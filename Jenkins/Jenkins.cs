using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;

namespace Leeroy
{
    public class Jenkins
    {
        public enum State
        {
            Unknown,
            Succeeded,
            Failed,
            RebuildingAfterFailed
        }

        private string _hostAndPort;

        public Jenkins(string hostAndPort)
        {
            _hostAndPort = hostAndPort;
        }
        
        public async Task<State> CheckState(string jobName)
        {
            using (HttpClient client = new HttpClient())
            {
                Uri uri = new Uri(string.Format("http://{0}/job/{1}/api/json", _hostAndPort, jobName));
                HttpResponseMessage response = await client.GetAsync(uri);
                if (!response.IsSuccessStatusCode)
                {
                    return State.Unknown;
                }

                JObject buildState = JObject.Parse(await response.Content.ReadAsStringAsync());
                int lastBuild = int.Parse(buildState["lastBuild"]["number"].ToString());
                int lastCompletedBuild = int.Parse(buildState["lastCompletedBuild"]["number"].ToString());
                int lastSuccessfulBuild = int.Parse(buildState["lastSuccessfulBuild"]["number"].ToString());
                bool isBuilding = lastBuild > lastCompletedBuild;
                bool isLastFailed = lastCompletedBuild > lastSuccessfulBuild;

                State state;
                if (!isLastFailed)
                {
                    state = State.Succeeded;
                }
                else if (!isBuilding)
                {
                    state = State.Failed;
                }
                else
                {
                    state = State.RebuildingAfterFailed;
                }
                return state;
            }
        }
    }
}
