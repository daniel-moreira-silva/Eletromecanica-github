export default {
  install(app) {
    app.directive("columns-resizable", {
      mounted(el, binding) {
        // primeira montagem
        if (!binding.value) return;
        applyResizable(el);
      },

      updated(el, binding) {
        // re-render/atualização do DOM
        if (!binding.value) {
          cleanup(el);
          return;
        }

        // se já existe, recria para refletir colunas novas
        cleanup(el);
        applyResizable(el);
      },

      beforeUnmount(el) {
        cleanup(el);
      },
    });
  },
};

/**
 * ===== Helpers =====
 * Mantém um estado por elemento para limpar corretamente.
 */
function cleanup(el) {
  const state = el.__colResizeState;
  if (!state) return;

  try {
    // remove listeners globais
    document.removeEventListener("mouseup", state.onMouseUp);

    // remove listeners locais
    if (state.resizeContainer && state.handleResize) {
      state.resizeContainer.removeEventListener("mousemove", state.handleResize);
    }
    if (state.table && state.handleResize) {
      state.table.removeEventListener("mousemove", state.handleResize);
    }

    // remove container
    if (state.resizeContainer && state.resizeContainer.parentElement) {
      state.resizeContainer.parentElement.removeChild(state.resizeContainer);
    }
  } catch (e) {
    // ignore
  }

  delete el.__colResizeState;
}

function applyResizable(el) {
  // remove barras antigas do documento (seu código fazia global)
  // Mantive, mas é um pouco agressivo (remove de todas as tabelas).
  // Se quiser, dá pra limitar só ao container criado.
  const columnsAsIs = document.querySelectorAll(".columns-resize-bar");
  columnsAsIs.forEach((x) => x.remove());

  const nodeName = el.nodeName;
  if (["TABLE", "THEAD"].indexOf(nodeName) < 0) return;

  const table = nodeName === "TABLE" ? el : el.parentElement;
  const thead = table.querySelector("thead");
  if (!thead) return;

  const ths = thead.querySelectorAll(".th-title");
  if (!ths?.length) return;

  const barHeight = nodeName === "TABLE" ? table.clientHeight : thead.offsetHeight;

  const resizeContainer = document.createElement("div");
  table.style.position = "relative";
  resizeContainer.style.position = "relative";
  resizeContainer.style.width = table.offsetWidth + "px";
  resizeContainer.className = "vue-columns-resizable";
  table.parentElement.insertBefore(resizeContainer, table);

  let moving = false;
  let doubleClick = false;
  let movingIndex = 0;

  ths.forEach((th, index) => {
    th.style.width = th.offsetWidth + "px";

    if (index + 1 >= ths.length) return;

    const nextTh = ths[index + 1];
    const bar = document.createElement("div");

    bar.style.position = "absolute";
    bar.style.left = getOffSetLeft(nextTh) + "px";
    bar.style.top = 0;
    bar.style.height = barHeight + "px";
    bar.style.width = "8px";
    bar.style.cursor = "col-resize";
    bar.style.zIndex = 2;
    bar.className = "columns-resize-bar";

    bar.addEventListener("mousedown", () => {
      moving = true;
      movingIndex = index;
      document.body.style.cursor = "col-resize";
      document.body.style.userSelect = "none";
    });

    bar.addEventListener("dblclick", () => {
      moving = false;
      doubleClick = true;
      const th = ths[index];

      if (th.children.length > 0) {
        if (th.children[0].className == "title-container") {
          const spanWidth = th.children[0].children[0].clientWidth;
          const divWidth = th.children[0].children[1].clientWidth;
          th.style.width = spanWidth + divWidth + "px";
        } else {
          th.style.width = "1px";
        }
      } else {
        th.style.width = "1px";
      }

      bars.forEach((innerBar, innerIndex) => {
        const nextTh = ths[innerIndex + 1];
        innerBar.style.left = getOffSetLeft(nextTh) + "px";
      });
    });

    resizeContainer.appendChild(bar);
  });

  const bars = resizeContainer.querySelectorAll(".columns-resize-bar");

  const cutPx = (str) => +String(str || "0").replace("px", "");

  const handleResize = (e) => {
    if (moving) {
      const th = ths[movingIndex];
      const nextTh = ths[movingIndex + 1];
      const bar = bars[movingIndex];

      th.style.width = cutPx(th.style.width) + e.movementX + "px";
      nextTh.style.width = cutPx(nextTh.style.width) - e.movementX + "px";
      bar.style.left = getOffSetLeft(nextTh) + e.movementX + "px";
    }
  };

  const onMouseUp = () => {
    if (!moving) return;

    if (doubleClick) {
      document.body.style.cursor = "";
      document.body.style.userSelect = "";
      doubleClick = false;
      moving = false;
      return;
    }

    moving = false;
    doubleClick = false;
    document.body.style.cursor = "";
    document.body.style.userSelect = "";

    bars.forEach((bar, index) => {
      const th = ths[index];
      const nextTh = ths[index + 1];
      th.style.width = th.offsetWidth + "px";
      bar.style.left = getOffSetLeft(nextTh) + "px";
    });
  };

  document.addEventListener("mouseup", onMouseUp);
  resizeContainer.addEventListener("mousemove", handleResize);
  table.addEventListener("mousemove", handleResize);

  // guarda estado para cleanup no unmount/disable
  el.__colResizeState = {
    table,
    resizeContainer,
    handleResize,
    onMouseUp,
  };

  function getOffSetLeft(nextTh) {
    return nextTh.offsetLeft - 4;
  }
}
