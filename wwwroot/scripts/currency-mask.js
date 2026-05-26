window.CurrencyMask = {
    instances: {},

    init: function (inputId, dotnetRef) {
        const input = document.getElementById(inputId);
        if (!input) return;

        const format = (raw) => {
            // Garde chiffres + séparateur décimal
            let cleaned = raw.replace(/[^\d,\.]/g, '');
            // Normalise le point en virgule
            cleaned = cleaned.replace('.', ',');
            // Une seule virgule max
            const parts = cleaned.split(',');
            let intPart = parts[0];
            let decPart = parts.length > 1 ? parts[1].substring(0, 2) : null;
            // Séparateur de milliers (espace)
            let formatted = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
            if (decPart !== null) formatted += ',' + decPart;
            return { formatted, intPart, decPart };
        };

        const handler = function () {
            const cursorPos = input.selectionStart;
            const oldValue = input.value;

            // Mémorise combien de chiffres sont avant le curseur
            let digitsBefore = 0;
            for (let i = 0; i < cursorPos; i++) {
                if (/\d/.test(oldValue[i])) digitsBefore++;
            }

            const { formatted, intPart, decPart } = format(oldValue);
            input.value = formatted;

            // Repositionne le curseur après le même nb de chiffres
            let found = 0;
            let newPos = formatted.length;
            for (let i = 0; i < formatted.length; i++) {
                if (/\d/.test(formatted[i])) found++;
                if (found === digitsBefore) { newPos = i + 1; break; }
            }
            input.setSelectionRange(newPos, newPos);

            // Sync valeur décimale vers Blazor
            const numStr = intPart + (decPart !== null ? '.' + decPart : '');
            const decimal = parseFloat(numStr) || 0;
            dotnetRef.invokeMethodAsync('OnJsValueChanged', decimal);
        };

        input.addEventListener('input', handler);
        input.addEventListener('paste', handler);
        this.instances[inputId] = handler;
    },

    setValue: function (inputId, value) {
        const input = document.getElementById(inputId);
        if (!input) return;
        if (!value || value === 0) { input.value = ''; return; }
        const parts = value.toFixed(2).split('.');
        const formatted = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ' ')
            + ',' + parts[1];
        input.value = formatted;
    },

    destroy: function (inputId) {
        const input = document.getElementById(inputId);
        const handler = this.instances[inputId];
        if (input && handler) input.removeEventListener('input', handler);
        delete this.instances[inputId];
    }
};