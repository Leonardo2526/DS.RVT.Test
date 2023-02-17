using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DataLib;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class PathCreatorClient
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private XYZ _startPoint;
        private XYZ _endPoint;
        private Line _line;

        public PathCreatorClient(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            //GetInputObjects();
        }


        public List<XYZ> GetPath()
        {
            var task = new RequestTask(_line, _startPoint, _endPoint);

            HttpClient httpClient = new HttpClient();

            Task task1 = Task.Run(async () => await CheckServer(httpClient));
            task1.Wait();

            var response = GetResponse(task, httpClient);

            return null;
            //return response.Result;
        }


        private void GetInputObjects()
        {
            ObjectSnapTypes snapTypes = ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections;

            var ref1 = _uiDoc.Selection.PickObject(ObjectType.PointOnElement, "Select startPoint");
            MEPCurve mEPCurve1 = _doc.GetElement(ref1) as MEPCurve;
            var ref2 = _uiDoc.Selection.PickObject(ObjectType.PointOnElement, "Select endPoint");
            MEPCurve mEPCurve2 = _doc.GetElement(ref2) as MEPCurve;
            _uiDoc.Selection.SetElementIds(new List<ElementId> { mEPCurve1.Id });
            _uiDoc.RefreshActiveView();

            _startPoint = ref1.GlobalPoint;
            _endPoint = ref2.GlobalPoint;
            _line = mEPCurve1.GetCenterLine();
        }


        private async Task<List<XYZ>> GetResponse(RequestTask task, HttpClient httpClient)
        {
            // создаем JsonContent
            JsonContent content = JsonContent.Create(task);

            // отправляем запрос
            using var response = await httpClient.PostAsync("http://localhost:5000/", content);
            Response? responseContent = await response.Content.ReadFromJsonAsync<Response>();

            return responseContent.Points;
        }

        public static async Task CheckServer(HttpClient httpClient)
        {
            string requestUri = "http://localhost:5000/";

            // определяем данные запроса
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            Console.WriteLine(request.RequestUri);

            // получаем ответ
            using HttpResponseMessage response = await httpClient.SendAsync(request);

            // просматриваем данные ответа
            // статус
            Debug.WriteLine($"Status: {response.StatusCode}\n");

            // содержимое ответа
            Debug.WriteLine("\nContent");
            string content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);
        }
    }
}
