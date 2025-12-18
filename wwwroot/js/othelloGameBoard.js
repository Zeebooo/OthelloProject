
function updateOthelloGameBoard() {
    console.log("Othello script loaded"); 
    fetch(`/Games/OthelloGameBoard`)
        .then(r => r.text())
        .then(html => {
            const container = document.getElementById("othelloGameBoardContainer");
            if (container) {
                container.innerHTML = html;
            }
        })
        .catch(err => console.error(err));
}
setInterval(updateOthelloGameBoard, 2000);