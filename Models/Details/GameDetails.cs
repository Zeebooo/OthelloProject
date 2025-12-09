namespace OthelloProject.Models.Methods
{
	public class GameDetails
	{
		public GameDetails(){}

		public int GameID { get; set; }
		public int User1ID { get; set; }
		public int? User2ID { get; set; }
		public string GameStatus { get; set; }
		public string Board { get; set; }
		public int? WinnerID { get; set; }
		public string? Username { get; set; }
	}
}