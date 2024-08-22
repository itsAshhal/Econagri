mergeInto(LibraryManager.library, {
    ShareOnWebGLJS: function(text, imageBase64) {
        if (navigator.share) {
            // Convert base64 string to blob
            const byteCharacters = atob(UTF8ToString(imageBase64));
            const byteNumbers = new Array(byteCharacters.length);
            for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            const blob = new Blob([byteArray], {type: 'image/png'});

            // Create a URL for the blob
            const imageUrl = URL.createObjectURL(blob);

            // Share text and image
            navigator.share({
                title: 'Shared from Unity WebGL',
                text: UTF8ToString(text),
                files: [imageUrl]
            })
                .then(() => {
                    console.log('Share successful');
                })
                .catch((error) => {
                    console.log('Share failed: ' + error);
                });
        } else {
            console.log('Web Share API not supported');
        }
    },
});
