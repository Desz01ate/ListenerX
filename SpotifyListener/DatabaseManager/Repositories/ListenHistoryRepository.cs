// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a ModelGenerator.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
using Utilities.Interfaces;
using SpotifyListener.DatabaseManager.Repositories.Based;
using SpotifyListener.DatabaseManager.Models;
namespace SpotifyListener.DatabaseManager.Repositories
{
	///<summary>
	/// Data contractor for ListenHistory
	///</summary>
	public partial class ListenHistoryRepository : Repository<ListenHistory,System.Data.SQLite.SQLiteConnection,System.Data.SQLite.SQLiteParameter>
	{
		public ListenHistoryRepository(IDatabaseConnectorExtension<System.Data.SQLite.SQLiteConnection,System.Data.SQLite.SQLiteParameter> connector) : base(connector)
		{
		}
	}
}

