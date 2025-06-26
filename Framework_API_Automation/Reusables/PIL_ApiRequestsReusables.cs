using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Framework_API_Automation.Utilities.Reporting;
using System.Net;

namespace Framework_API_Automation.Reusables
{
    public class PIL_ApiRequestsReusables
    {



        private static readonly CookieContainer cookieContainer = new CookieContainer();

        public static HttpClient RestClient { get; private set; }
        public static HttpClient GraphQLClient { get; private set; }

        public static void InitializeClients()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };

            // Initialize REST Client
            RestClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://pi-stage.serviceranger4.com/graphql")
            };
            RestClient.DefaultRequestHeaders.Accept.Clear();
            RestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Use the same client for GraphQL to share cookies
            GraphQLClient = RestClient;
        }


        public static async Task<HttpResponseMessage> SendPostRequest(string url, string authType, string token, string value, string username, string password, string apiKeyName, int? timeoutInSeconds, string jsonBody, string cookies)
        {
            try
            {
                ExtentReportHelper.GetTest()?.Info("Sending POST request to Endpoint : " + url);
                var client = GraphQLClient; // ✅ Use shared client with cookie container

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                // Add headers
                if (!string.IsNullOrEmpty(cookies))
                {
                    request.Headers.Add("Cookie", cookies);
                    ExtentReportHelper.LogPass("Cookies: " + cookies);
                }

                if (!string.IsNullOrEmpty(authType))
                {
                    switch (authType.ToLower())
                    {
                        case "bearer":
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            ExtentReportHelper.LogPass("Authorization Header: Bearer " + token);
                            break;
                        case "apikey":
                            if (!string.IsNullOrEmpty(apiKeyName))
                            {
                                request.Headers.Add(apiKeyName, value);
                                ExtentReportHelper.LogPass("Authorization Header: API Key " + apiKeyName + ": " + value);
                            }
                            else
                            {
                                ExtentReportHelper.LogSkip("API Key name is missing for authType 'apikey'");
                            }
                            break;
                        case "basic":
                            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                            {
                                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                                ExtentReportHelper.LogPass("Authorization Header: Basic " + authValue);
                            }
                            else
                            {
                                ExtentReportHelper.LogSkip("Username or password is missing for authType 'basic'");
                            }
                            break;
                        default:
                            ExtentReportHelper.LogSkip("Unknown authorization type: " + authType);
                            break;
                    }
                }

                var startTime = DateTime.Now;
                var response = await RestClient.SendAsync(request);
                var duration = (DateTime.Now - startTime).TotalSeconds;

                var statusCode = (int)response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();

                ExtentReportHelper.LogPass("Received response with status code: " + statusCode);
                ExtentReportHelper.LogPass("Response body: " + responseBody);
                ExtentReportHelper.LogPass("Response time: " + duration + " seconds");

                if (timeoutInSeconds.HasValue && duration > timeoutInSeconds.Value)
                {
                    var errormsg = "Error: Response time exceeded " + timeoutInSeconds + " seconds";
                    ExtentReportHelper.LogFail(errormsg);
                    throw new TimeoutException(errormsg);
                }

                //if (statusCode >= 200 && statusCode < 300)
                //{
                //    bool isValidStatusCode = statusCode == 200 || statusCode == 201 || statusCode == 203 || statusCode == 204;
                //    if (isValidStatusCode)        
                //    ExtentReportHelper.LogPass("Success: " + statusCode + " " + response.ReasonPhrase);
                //}

                if ((int)statusCode >= 200 && (int)statusCode < 300)
                    ExtentReportHelper.LogPass("Success: " + (int)response.StatusCode + " " + response.ReasonPhrase);
                return response;
            }
            catch (Exception e)
            {
                ExtentReportHelper.LogFail("Error during request", e);
                throw;
            }
        }



        public static async Task<HttpResponseMessage> SendPostRequestWithGraphqlAsync(string url,string authType,string token,string value,string username,string password,string apiKeyName,int? timeoutInSeconds,string graphqlQuery,string graphqlVariablesJson, string cookies)
        {
            try
            {
                ExtentReportHelper.GetTest()?.Info("Sending GraphQL POST request to Endpoint : " + url);             
                var client = GraphQLClient; // ✅ Use shared client with cookie container

                var graphqlBody = new JObject
                {
                    ["query"] = graphqlQuery,
                    ["variables"] = JObject.Parse(graphqlVariablesJson ?? "{}")
                };

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(graphqlBody.ToString(), Encoding.UTF8, "application/json")
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(cookies))
                {
                    request.Headers.Add("Cookie", cookies);
                    ExtentReportHelper.LogPass("Cookies: " + cookies);
                }

                if (!string.IsNullOrEmpty(authType))
                {
                    switch (authType.ToLower())
                    {
                        case "bearer":
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            break;
                        case "apikey":
                            if (!string.IsNullOrEmpty(apiKeyName))
                                request.Headers.Add(apiKeyName, value);
                            break;
                        case "basic":
                            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                            {
                                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                            }
                            break;
                    }
                }

                ExtentReportHelper.LogPass("GraphQL Request Body:\n" + graphqlBody.ToString(Newtonsoft.Json.Formatting.Indented));
                ExtentReportHelper.LogPass("Request Headers:\n" + string.Join("\n", request.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)}")));

                var startTime = DateTime.Now;
                var response = await client.SendAsync(request);
                var duration = (DateTime.Now - startTime).TotalSeconds;

                var responseBody = await response.Content.ReadAsStringAsync();
                ExtentReportHelper.LogPass("Received response with status code: " + (int)response.StatusCode);
                ExtentReportHelper.LogPass("Response body: " + responseBody);

                ExtentReportHelper.LogPass("Response time: " + duration + " seconds");

                if (timeoutInSeconds.HasValue && duration > timeoutInSeconds.Value)
                    throw new TimeoutException($"Response time exceeded {timeoutInSeconds} seconds");

                if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                    ExtentReportHelper.LogPass("Success: " + (int)response.StatusCode + " " + response.ReasonPhrase);              
                return response;
            }
            catch (Exception e)
            {
                ExtentReportHelper.LogFail("Error during GraphQL request", e);
                throw;
            }
        }





        public static async Task<HttpResponseMessage> SendGetRequestAsync(string url, string authType, string token, string value, string username, string password, string apiKeyName, int? timeoutInSeconds, string cookies)
        {
            try
            {
                ExtentReportHelper.GetTest()?.Info("Sending GET request" + url);


                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // Add headers
                request.Headers.Add("Accept", "application/json");

                if (!string.IsNullOrEmpty(cookies))
                {
                    request.Headers.Add("Cookie", cookies);
                    ExtentReportHelper.LogPass("Cookies: " + cookies);
                }

                if (!string.IsNullOrEmpty(authType))
                {
                    switch (authType.ToLower())
                    {
                        case "bearer":
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            ExtentReportHelper.LogPass("Authorization Header: Bearer " + token);
                            break;
                        case "apikey":
                            if (!string.IsNullOrEmpty(apiKeyName))
                            {
                                request.Headers.Add(apiKeyName, value);
                                ExtentReportHelper.LogPass("Authorization Header: API Key " + apiKeyName + ": " + value);
                            }
                            else
                            {
                                ExtentReportHelper.LogSkip("API Key name is missing for authType 'apikey'");
                            }
                            break;
                        case "basic":
                            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                            {
                                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                                ExtentReportHelper.LogPass("Authorization Header: Basic " + authValue);
                            }
                            else
                            {
                                ExtentReportHelper.LogSkip("Username or password is missing for authType 'basic'");
                            }
                            break;
                        default:
                            ExtentReportHelper.LogSkip("Unknown authorization type: " + authType);
                            break;
                    }
                }

                var startTime = DateTime.Now;
                var response = await RestClient.SendAsync(request);
                var duration = (DateTime.Now - startTime).TotalSeconds;

                var responseBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                ExtentReportHelper.LogPass("Received response with status code: " + statusCode);
                ExtentReportHelper.LogPass("Response body: " + responseBody);
                ExtentReportHelper.LogPass("Response time: " + duration + " seconds");

                if (timeoutInSeconds.HasValue && duration > timeoutInSeconds.Value)
                {
                    var errormsg = "Error: Response time exceeded " + timeoutInSeconds + " seconds";
                    ExtentReportHelper.LogFail(errormsg);
                    throw new TimeoutException(errormsg);
                }

                if (statusCode >= 200 && statusCode < 300)
                {
                    bool isValidStatusCode = statusCode == 200 || statusCode == 201 || statusCode == 203 || statusCode == 204;
                    if (!isValidStatusCode)
                        throw new Exception("Unexpected success status code: " + statusCode);

                    ExtentReportHelper.LogPass("Success: " + statusCode + " " + response.ReasonPhrase);
                }            
                else
                {
                    ExtentReportHelper.LogFail("Unexpected Error: " + statusCode + " " + response.ReasonPhrase);
                }

                return response;
            }
            catch (Exception e)
            {
                ExtentReportHelper.LogFail("Error during request", e);
                throw;
            }
        }





        public static async Task HandleResponseAsync(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            if (responseBody.Contains("Success"))
            {
                ExtentReportHelper.LogPass("Response verified successfully.");
            }
            else
            {
                ExtentReportHelper.LogFail("Response not verified: fail");
            }
            ExtentReportHelper.LogFail(responseBody);
        }



        public static async Task<HttpResponseMessage> SendDeleteRequestAsync(string url, string authType, string token, string username, string password, string apiKeyName, int? timeoutInSeconds, string cookies)
        {
            try
            {
                ExtentReportHelper.GetTest()?.Info("Sending DELETE request" + url);

                var request = new HttpRequestMessage(HttpMethod.Delete, url);

                if (!string.IsNullOrEmpty(cookies))
                {
                    request.Headers.Add("Cookie", cookies);
                    ExtentReportHelper.LogPass("Cookies: " + cookies);
                }

                if (!string.IsNullOrEmpty(authType))
                {
                    switch (authType.ToLower())
                    {
                        case "bearer":
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            break;
                        case "apikey":
                            if (!string.IsNullOrEmpty(apiKeyName))
                                request.Headers.Add(apiKeyName, token);
                            break;
                        case "basic":
                            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                            {
                                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                            }
                            break;
                    }
                }

                var startTime = DateTime.Now;
                var response = await RestClient.SendAsync(request);
                var duration = (DateTime.Now - startTime).TotalSeconds;

                var responseBody = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                ExtentReportHelper.LogPass("Status: " + statusCode);
                ExtentReportHelper.LogPass("Body: " + responseBody);
                ExtentReportHelper.LogPass("Time: " + duration + " seconds");

                if (timeoutInSeconds.HasValue && duration > timeoutInSeconds.Value)
                {
                    throw new TimeoutException("Response time exceeded " + timeoutInSeconds + " seconds");
                }

                if (statusCode < 200 || statusCode > 300)
                {
                    ExtentReportHelper.LogFail("Error: " + statusCode + " " + response.ReasonPhrase);
                }

                return response;
            }
            catch (Exception ex)
            {
                ExtentReportHelper.LogFail("Exception during DELETE request", ex);
                throw;
            }

        }


        public static async Task<HttpResponseMessage> SendPutRequestAsync(string url, string authType, string token, string value, string username, string password, string apiKeyName, int? timeoutInSeconds, string jsonBody, string cookies)
        {
            try
            {
                ExtentReportHelper.GetTest()?.Info("Sending Put request" + url);


                var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                // Add headers
                if (!string.IsNullOrEmpty(cookies))
                {
                    request.Headers.Add("Cookie", cookies);
                    ExtentReportHelper.LogPass("Cookies: " + cookies);
                }

                if (!string.IsNullOrEmpty(authType))
                {
                    switch (authType.ToLower())
                    {
                        case "bearer":
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            ExtentReportHelper.LogPass("Authorization Header: Bearer " + token);
                            break;
                        case "apikey":
                            if (!string.IsNullOrEmpty(apiKeyName))
                            {
                                request.Headers.Add(apiKeyName, value);
                                ExtentReportHelper.LogPass("Authorization Header: API Key " + apiKeyName + ": " + value);
                            }
                            else
                            {
                                ExtentReportHelper.LogSkip("API Key name is missing for authType 'apikey'");
                            }
                            break;
                        case "basic":
                            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                            {
                                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                                ExtentReportHelper.LogPass("Authorization Header: Basic " + authValue);
                            }
                            else
                            {
                                ExtentReportHelper.LogSkip("Username or password is missing for authType 'basic'");
                            }
                            break;
                        default:
                            ExtentReportHelper.LogSkip("Unknown authorization type: " + authType);
                            break;
                    }
                }

                var startTime = DateTime.Now;
                var response = await RestClient.SendAsync(request);
                var duration = (DateTime.Now - startTime).TotalSeconds;

                var statusCode = (int)response.StatusCode;
                var responseBody = await response.Content.ReadAsStringAsync();

                ExtentReportHelper.LogPass("Received response with status code: " + statusCode);
                ExtentReportHelper.LogPass("Response body: " + responseBody);
                ExtentReportHelper.LogPass("Response time: " + duration + " seconds");

                if (timeoutInSeconds.HasValue && duration > timeoutInSeconds.Value)
                {
                    var errormsg = "Error: Response time exceeded " + timeoutInSeconds + " seconds";
                    ExtentReportHelper.LogFail(errormsg);
                    throw new TimeoutException(errormsg);
                }

                if (statusCode >= 200 && statusCode < 300)
                {
                    bool isValidStatusCode = statusCode == 200 || statusCode == 201 || statusCode == 203 || statusCode == 204;
                    if (!isValidStatusCode)
                        throw new Exception("Unexpected success status code: " + statusCode);

                    ExtentReportHelper.LogPass("Success: " + statusCode + " " + response.ReasonPhrase);
                }               
                else
                {
                    ExtentReportHelper.LogFail("Unexpected Error: " + statusCode + " " + response.ReasonPhrase + " " + responseBody);
                }

                return response;
            }
            catch (Exception e)
            {
                ExtentReportHelper.LogFail("Error during request", e);
                throw;
            }
        }

       
    }
}
