async function GenerateObjectURL(contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
          
    return url;
}

function RevokeURL(url) {
    URL.revokeObjectURL(url);
}

function ScrollToTop(){
    document.getElementById('content').scrollTop = 0;
}

function DownloadPhoto(url) {
    const downloadLink = document.createElement("a");
    downloadLink.href = url;

    downloadLink.click();

    downloadLink.remove();
}