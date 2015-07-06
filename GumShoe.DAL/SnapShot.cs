using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GumShoe.DAL
{
    public class SnapShot
    {
        #region Private Properties
        private readonly string _databaseFileName;

        private const string SqlGetLastInsertedId = "select last_insert_rowid()";
        private const string SqlInsertSnapShot =
            "Insert into SnapShot (Date, SeedUrl, Delay, Steps) values (@Date, @SeedUrl, @Delay, @Steps)";

        private const string SqlInsertPageContent =
            "Insert into PageContent (SnapShotId, Date, Domain, Path, Querystring, PageText) " +
            "values (@SnapShotId, @Date, @Domain, @Path, @Querystring, @PageText)";

        #endregion

        #region Public Members
        /// <summary>
        /// Instantiate the SnapShot class and set the databaseFileName that will be used for this instance
        /// </summary>
        /// <param name="databaseFileName">The name of the file being accessed</param>
        public SnapShot(string databaseFileName)
        {
            _databaseFileName = databaseFileName;
        }

        /// <summary>
        /// Insert a SnapShot record in to the database file set on instantiation
        /// </summary>
        /// <param name="seedUrl">Where the current crawl started</param>
        /// <param name="delay">The delay in seconds between hitting the site</param>
        /// <param name="steps">The number of steps away from the seedUrl to transverse</param>
        /// <returns>SnapShotId is returned</returns>
        public long InsertSnapShot(string seedUrl, int delay, int steps)
        {
            long snapShotId = 0;
            using (var con = new SQLiteConnection("data source=" + _databaseFileName))
            {
                var com = new SQLiteCommand(con);
                con.Open();
                com.CommandText = SqlInsertSnapShot;
                com.Parameters.AddWithValue("@Date", DateTime.Now);
                com.Parameters.AddWithValue("@SeedUrl", seedUrl);
                com.Parameters.AddWithValue("@Delay", delay);
                com.Parameters.AddWithValue("@Steps", steps);
                com.ExecuteNonQuery();
                snapShotId = getLastId(com);
                con.Close();
            }
            return snapShotId;
        }

        /// <summary>
        /// Insert a PageContent record. This should contain the text pulled from a page not the full html
        /// </summary>
        /// <param name="snapShotId">Which crawl does this content belong to</param>
        /// <param name="uri">The domain, path, and querystring are pulled from this</param>
        /// <param name="text">The text from a loaded html document with single spaces between words</param>
        /// <returns>PageContentId is returned</returns>
        public long InsertPageContent(long snapShotId, Uri uri, string text)
        {
            var domain = uri.Scheme + "://" + uri.Authority;  // want the domain name with scheme in case some parts are linked as https
            var path = uri.LocalPath;
            var querystring = uri.Query;
            long pageContentId = 0;
            using (var con = new SQLiteConnection("data source=" + _databaseFileName))
            {
                var com = new SQLiteCommand(con);
                con.Open();
                com.CommandText = SqlInsertPageContent;
                com.Parameters.AddWithValue("@SnapShotId", snapShotId);
                com.Parameters.AddWithValue("@Date", DateTime.Now);
                com.Parameters.AddWithValue("@Domain", domain);
                com.Parameters.AddWithValue("@Path", path);
                com.Parameters.AddWithValue("@Querystring", querystring);
                com.Parameters.AddWithValue("@PageText", text);
                com.ExecuteNonQuery();
                pageContentId = getLastId(com);
                con.Close();
            }
            return pageContentId;
        }
        #endregion

        #region Private Members
        private long getLastId(SQLiteCommand com)
        {
            com.CommandText = SqlGetLastInsertedId;
            var lastId = (long) com.ExecuteScalar();
            return lastId;
        }
        #endregion
    }
}
