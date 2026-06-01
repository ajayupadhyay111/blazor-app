// Thin localStorage wrapper used by DraftStore via JS interop.
// Kept dependency-free on purpose so the app needs no extra NuGet package.
window.appStorage = {
    save: function (key, value) {
        try { localStorage.setItem(key, value); return true; }
        catch (e) { console.error('storage.save failed', e); return false; }
    },
    load: function (key) {
        return localStorage.getItem(key);
    },
    remove: function (key) {
        localStorage.removeItem(key);
    },
    print: function () { window.print(); }
};
