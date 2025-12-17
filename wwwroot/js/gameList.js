function updateGameList() {
    fetch(`/Games/GameList?sorted=${sorted}`)
        .then(r => r.text())
        .then(html => {
            const container = document.getElementById("gameListContainer");
            if (container) {
                container.innerHTML = html;
            }
        })
        .catch(err => console.error(err));
}
setInterval(updateGameList, 2000);