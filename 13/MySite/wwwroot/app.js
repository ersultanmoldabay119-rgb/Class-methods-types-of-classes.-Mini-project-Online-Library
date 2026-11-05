"use strict";

const $ = (id) => document.getElementById(id);

// ───────────── Запросы к серверу ─────────────

async function api(path, body) {
  const options = body === undefined
    ? {}
    : { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) };

  const response = await fetch("/api" + path, options);
  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(data?.message ?? "Ошибка сервера (" + response.status + ")");
  }
  return data;
}

// ───────────── Вспомогательное ─────────────

function el(tag, props = {}, ...children) {
  const node = document.createElement(tag);
  Object.assign(node, props);
  for (const child of children) {
    if (child != null) node.append(child);
  }
  return node;
}

function yearsWord(n) {
  const lastTwo = Math.abs(n) % 100;
  const last = lastTwo % 10;
  if (lastTwo >= 11 && lastTwo <= 14) return "лет";
  if (last === 1) return "год";
  if (last >= 2 && last <= 4) return "года";
  return "лет";
}

let toastTimer;
function showToast(message, isError = false) {
  const toast = $("toast");
  toast.textContent = message;
  toast.classList.toggle("error", isError);
  toast.hidden = false;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => (toast.hidden = true), 4500);
}

function fillSelect(select, items, placeholder) {
  const previous = select.value;
  select.replaceChildren(el("option", { value: "", textContent: placeholder, disabled: true }));

  for (const item of items) {
    select.append(el("option", { value: item.value, textContent: item.label }));
  }

  const stillThere = items.some((i) => i.value === previous);
  select.value = stillThere ? previous : "";
  select.disabled = items.length === 0;
}

// ───────────── Отрисовка ─────────────

function statusBadge(book) {
  if (book.available) return el("span", { className: "badge", textContent: "доступна" });
  if (book.overdue) return el("span", { className: "badge overdue", textContent: "просрочена" });
  return el("span", { className: "badge borrowed", textContent: "выдана" });
}

function renderBooks(books) {
  const body = $("books-body");
  body.replaceChildren();

  for (const book of books) {
    const action = book.available
      ? el("button", { className: "ghost", type: "button", textContent: "Выдать", onclick: () => pick("borrow-book", book.title) })
      : el("button", { className: "ghost", type: "button", textContent: "Вернуть", onclick: () => pick("return-book", book.title) });

    const editButton = el("button", {
      className: "ghost", type: "button", textContent: "Описание",
      onclick: () => openDescription(book),
    });

    const description = book.description
      ? el("p", {
          className: "desc", textContent: book.description, title: "Нажмите, чтобы развернуть",
          onclick: (e) => e.currentTarget.classList.toggle("expanded"),
        })
      : el("p", { className: "desc none", textContent: "Описания нет" });

    const statusCell = el("td", {}, statusBadge(book));
    if (!book.available) {
      statusCell.append(el("span", { className: "sub", textContent: book.borrower + ", до " + book.dueDate }));
    }

    body.append(el("tr", {},
      el("td", {},
        el("span", { className: "title", textContent: book.title }),
        el("span", { className: "sub author-sub", textContent: book.author }),
        description),
      el("td", { className: "author-col", textContent: book.author }),
      el("td", { className: "year" },
        String(book.year),
        el("span", { className: "sub", textContent: book.age + " " + yearsWord(book.age) })),
      statusCell,
      el("td", {}, el("div", { className: "actions" }, action, editButton))));
  }

  $("books-empty").hidden = books.length > 0;
  $("books-count").textContent = books.length ? "показано: " + books.length : "";
}

function renderReaders(readers) {
  const list = $("readers-list");
  list.replaceChildren();

  for (const reader of readers) {
    const head = el("div", { className: "reader-head" },
      el("span", { className: "reader-name", textContent: reader.fullName }),
      el("span", { className: "muted", textContent: reader.phone ?? "" }));

    const item = el("li", {}, head);

    if (reader.books.length === 0) {
      item.append(el("div", { className: "muted", textContent: "Книг на руках нет" }));
    } else {
      const ul = el("ul", { className: "reader-books" });
      for (const b of reader.books) {
        const text = "«" + b.title + "» — вернуть до " + b.dueDate + (b.overdue ? " (просрочена)" : "");
        ul.append(el("li", { textContent: text }));
      }
      item.append(ul);
    }

    list.append(item);
  }

  $("readers-empty").hidden = readers.length > 0;
  $("readers-count").textContent = readers.length ? "всего: " + readers.length : "";
}

function renderStats(s) {
  $("stat-total").textContent = s.totalBooks;
  $("stat-available").textContent = s.availableBooks;
  $("stat-borrowed").textContent = s.borrowedBooks;
  $("stat-overdue").textContent = s.overdueBooks;
  $("stat-overdue").classList.toggle("has-overdue", s.overdueBooks > 0);
  $("stat-readers").textContent = s.readers;
}

function renderSelects(allBooks, readers) {
  fillSelect($("borrow-book"),
    allBooks.filter((b) => b.available).map((b) => ({ value: b.title, label: b.title })),
    "— выберите книгу —");

  fillSelect($("borrow-reader"),
    readers.map((r) => ({ value: r.fullName, label: r.fullName })),
    readers.length ? "— выберите читателя —" : "сначала зарегистрируйте читателя");

  const borrowed = allBooks.filter((b) => !b.available);
  fillSelect($("return-book"),
    borrowed.map((b) => ({ value: b.title, label: b.title + " (" + b.borrower + ")" })),
    borrowed.length ? "— выберите книгу —" : "выданных книг нет");

  updateReturnHint(borrowed);
}

let borrowedCache = [];
function updateReturnHint(borrowed = borrowedCache) {
  borrowedCache = borrowed;
  const book = borrowed.find((b) => b.title === $("return-book").value);
  $("return-hint").textContent = book ? "Выдана " + book.borrowDate + ", срок — до " + book.dueDate + "." : "";
}

function pick(selectId, title) {
  const select = $(selectId);
  select.value = title;
  updateReturnHint();
  select.closest(".panel").scrollIntoView({ behavior: "smooth", block: "center" });
  select.focus();
}

// ───────────── Загрузка данных ─────────────

async function refresh() {
  const q = $("search-q").value.trim();
  const params = new URLSearchParams();
  if (q) {
    params.set("q", q);
    params.set("by", $("search-by").value);
  }

  try {
    const [shown, all, readers, stats] = await Promise.all([
      api("/books?" + params),
      api("/books"),
      api("/readers"),
      api("/stats"),
    ]);
    renderBooks(shown);
    renderReaders(readers);
    renderStats(stats);
    renderSelects(all, readers);
  } catch (err) {
    showToast(err.message, true);
  }
}

// ───────────── Формы ─────────────

function handleForm(formId, path, toBody) {
  const form = $(formId);
  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const button = form.querySelector("button[type=submit]");
    button.disabled = true;
    try {
      const result = await api(path, toBody(new FormData(form)));
      showToast(result.message);
      form.reset();
      await refresh();
    } catch (err) {
      showToast(err.message, true);
    } finally {
      button.disabled = false;
    }
  });
}

const text = (v) => (v ?? "").toString().trim();
const numberOrNull = (v) => (text(v) === "" ? null : Number(v));

handleForm("add-book-form", "/books", (d) => ({
  title: text(d.get("title")),
  author: text(d.get("author")),
  year: numberOrNull(d.get("year")),
  description: text(d.get("description")) || null,
}));

// ───────────── Описание книги ─────────────

const descDialog = $("desc-dialog");
const descForm = $("desc-form");
const descText = descForm.elements.description;

function updateDescCounter() {
  $("desc-counter").textContent = descText.value.length + " / " + descText.maxLength;
}

function openDescription(book) {
  descForm.elements.title.value = book.title;
  descText.value = book.description ?? "";
  $("desc-book").textContent = "«" + book.title + "» — " + book.author + ", " + book.year;
  updateDescCounter();
  descDialog.showModal();
  descText.focus();
}

descText.addEventListener("input", updateDescCounter);
$("desc-cancel").addEventListener("click", () => descDialog.close());

descForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const button = descForm.querySelector("button[type=submit]");
  button.disabled = true;
  try {
    const result = await api("/books/description", {
      title: descForm.elements.title.value,
      description: text(descText.value),
    });
    descDialog.close();
    showToast(result.message);
    await refresh();
  } catch (err) {
    showToast(err.message, true);
  } finally {
    button.disabled = false;
  }
});

handleForm("add-reader-form", "/readers", (d) => ({
  firstName: text(d.get("firstName")),
  lastName: text(d.get("lastName")) || null,
  phone: text(d.get("phone")) || null,
}));

handleForm("borrow-form", "/borrow", (d) => ({
  title: d.get("title"),
  readerName: d.get("readerName"),
}));

handleForm("return-form", "/return", (d) => ({
  title: d.get("title"),
  daysKept: numberOrNull(d.get("daysKept")),
}));

// ───────────── Поиск ─────────────

let searchTimer;
$("search-form").addEventListener("submit", (e) => e.preventDefault());
$("search-q").addEventListener("input", () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(refresh, 250);
});
$("search-by").addEventListener("change", () => {
  $("search-q").inputMode = $("search-by").value === "year" ? "numeric" : "text";
  refresh();
});
$("sort-year").addEventListener("click", async () => {
  try {
    const result = await api("/books/sort", {});
    showToast(result.message);
    await refresh();
  } catch (err) {
    showToast(err.message, true);
  }
});
$("return-book").addEventListener("change", () => updateReturnHint());

// ───────────── Старт ─────────────

(async () => {
  try {
    const info = await api("/info");
    document.title = info.name;
    $("library-name").textContent = info.name;
    $("library-rules").textContent =
      "Книги выдаются на " + info.loanPeriodDays + " дней · штраф за просрочку " + info.finePerDay + " тг в день";
    document.querySelector("#add-book-form [name=year]").max = new Date().getFullYear();
  } catch (err) {
    showToast(err.message, true);
  }
  await refresh();
})();
