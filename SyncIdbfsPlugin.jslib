mergeInto(LibraryManager.library, {
    SyncIdbfs: function () {
        FS.syncfs(false, function (err) {
        });
    }
});