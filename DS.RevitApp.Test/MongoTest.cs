using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class MongoTest
    {
        public MongoTest()
        {
            Run();
        }

        private void Run()
        {
            var client = new MongoClient();
            var database = client.GetDatabase("testDb");
            Debug.WriteLine(database.DatabaseNamespace.DatabaseName);

            var collections = database.ListCollections();
            Debug.WriteLine(collections.ToList().Count);

            var document = new BsonDocument

{
    { "name", "MongoDB" },
    { "type", "Database" },
    { "count", 1 },
    { "info", new BsonDocument
        {
            { "x", 203 },
        { "y", 102 }
    }}
};
            var collection = database.GetCollection<BsonDocument>("name");
            //database.CreateCollection("newCollection");
            collection.InsertOne(document);
            FilterDefinitionBuilder<BsonDocument> filterDefinitionBuilder = new FilterDefinitionBuilder<BsonDocument>();
            FilterDefinition<BsonDocument> filterDefinition = filterDefinitionBuilder.Empty;
            var docsCount = collection.CountDocuments(filterDefinition);
            Debug.WriteLine("Documents count in 'name': " + docsCount);


            //collections = database.ListCollections();
            //Debug.WriteLine(collections.ToList().Count);
        }
    }
}
