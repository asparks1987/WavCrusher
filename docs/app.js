(() => {
  "use strict";

  const form = document.querySelector("[data-estimator]");
  if (!form) {
    return;
  }

  const sizeInput = form.querySelector("#library-size");
  const unitInput = form.querySelector("#library-unit");
  const ratioInput = form.querySelector("#ratio");
  const ratioOutput = form.querySelector("#ratio-output");
  const archiveOutput = form.querySelector("#archive-size");
  const savedOutput = form.querySelector("#saved-size");

  const format = (value, unit) => {
    const digits = value >= 100 ? 0 : value >= 10 ? 1 : 2;
    return `${value.toLocaleString(undefined, { maximumFractionDigits: digits })} ${unit}`;
  };

  const update = () => {
    const size = Math.max(0, Number.parseFloat(sizeInput.value) || 0);
    const ratio = Math.min(100, Math.max(0, Number.parseFloat(ratioInput.value) || 0));
    const unit = unitInput.value;
    const archive = size * (ratio / 100);
    const saved = Math.max(0, size - archive);

    ratioOutput.value = `${ratio.toFixed(0)}%`;
    ratioOutput.textContent = `${ratio.toFixed(0)}%`;
    archiveOutput.textContent = format(archive, unit);
    savedOutput.textContent = format(saved, unit);
  };

  [sizeInput, unitInput, ratioInput].forEach((control) => {
    control.addEventListener("input", update);
    control.addEventListener("change", update);
  });

  update();
})();
