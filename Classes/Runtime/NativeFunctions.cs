using Classes.AST;
using Classes.AST.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    internal class NativeFunctions
    {
        private readonly Dictionary<int, NativeHttpResponse> httpResponses = new();
        private int nextResId = 1;

        private Interpreter runtime;
        private ExpressionEvaluator evaluator;
        private HttpClient httpClient = new HttpClient();

        public NativeFunctions(Interpreter runtime, ExpressionEvaluator evaluator)
        {
            this.runtime = runtime;
            this.evaluator = evaluator;
        }

        public bool Exists(string name)
        {
            switch (name)
            {
                case string value when value == "__http_request":
                    {
                        return true;
                    }
                case string value when value == "__http_status":
                    {
                        return true;
                    }
                case string value when value == "__http_body":
                    {
                        return true;
                    }
                case string value when value == "__http_header":
                    {
                        return true;
                    }
                case string value when value == "__http_dispose":
                    {
                        return true;
                    }
                case string value when value == "__json_serialize":
                    {
                        return true;
                    }
                case string value when value == "__error_message":
                    {
                        return true;
                    }
                case string value when value == "__error_line":
                    {
                        return true;
                    }
                case string value when value == "__error_col":
                    {
                        return true;
                    }
                case string value when value == "__error_source":
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public object? Invoke(string name, List<Expression> args, Scope callerScope)
        {
            switch (name)
            {
                case string value when value == "__http_request":
                    {
                        return HttpRequest(args, callerScope);
                    }
                case string value when value == "__http_status":
                    {
                        return HttpStatus(args, callerScope);
                    }
                case string value when value == "__http_body":
                    {
                        return HttpBody(args, callerScope);
                    }
                case string value when value == "__http_header":
                    {
                        return HttpHeader(args, callerScope);
                    }
                case string value when value == "__http_dispose":
                    {
                        return HttpDispose(args, callerScope);
                    }
                case string value when value == "__json_serialize":
                    {
                        return JsonSerialize(args, callerScope);
                    }
                case string value when value == "__error_message":
                    {
                        return ErrorInfo(args, "message", "__error_message");
                    }
                case string value when value == "__error_line":
                    {
                        return ErrorInfo(args, "line", "__error_line");
                    }
                case string value when value == "__error_col":
                    {
                        return ErrorInfo(args, "col", "__error_col");
                    }
                case string value when value == "__error_source":
                    {
                        return ErrorInfo(args, "source", "__error_source");
                    }
                default:
                    {
                        throw runtime.Error($"Unknown native function {name}");
                    }
            }
        }

        private object ErrorInfo(List<Expression> args, string kind, string functionName)
        {
            if (args.Count != 0)
            {
                throw runtime.Error($"{functionName} expected 0 arguments");
            }

            return runtime.CaughtOrCurrent(kind);
        }

        private object JsonSerialize(List<Expression> args, Scope callerScope)
        {
            if(args.Count != 1)
            {
                throw runtime.Error("__json_serialize expected 1 argument");
            }

            object value = evaluator.Evaluate(args[0], callerScope);
            object? toSerialize = ConvertToJsonValue(value);

            string jsonObject = JsonSerializer.Serialize(toSerialize);
            return jsonObject;
        }

        private object? ConvertToJsonValue(object? value, List<ClassInstance>? visited = null)
        {
            if (value is null)
            {
                return null;
            }

            if (value is List<object> arr)
            {
                List<object?> result = new();

                foreach (object? element in arr)
                {
                    if (element is null)
                    {
                        result.Add(null);
                    }
                    else
                    {
                        result.Add(ConvertToJsonValue(element, visited));
                    }
                }

                return result;
            }

            if (value is ClassInstance classInstance)
            {
                visited ??= new();
                if (visited.Contains(classInstance))
                {
                    throw runtime.Error("Cannot serialize circular object reference");
                }
                visited.Add(classInstance);

                Dictionary<string, object?> fields = new();
                IEnumerable<KeyValuePair<string, RunTimeVariable>> variables = classInstance.fields.GetLocalVariables();
                IEnumerable<KeyValuePair<string, RunTimeObject>> objects = classInstance.fields.GetLocalObjects();

                foreach(KeyValuePair<string, RunTimeVariable> kvp in variables)
                {
                    if (runtime.TryFindPrimitiveField(classInstance.definition, kvp.Key, out DefineVar definition, out DefClass owner))
                    {
                        if (definition.visibility == Visibility.PUBLIC)
                        {
                            if (!kvp.Value.isInitialized)
                            {
                                throw runtime.Error($"Cannot serialize uninitialized field {kvp.Key}");
                            }
                            fields.Add(kvp.Key, ConvertToJsonValue(kvp.Value.value, visited));
                        }
                    }
                }

                foreach(KeyValuePair<string, RunTimeObject> kvp in objects)
                {
                    if (runtime.TryFindObjectField(classInstance.definition, kvp.Key, out DefineObject definition, out DefClass owner))
                    {
                        if (definition.visibility == Visibility.PUBLIC)
                        {
                            object? objectValue = definition.isArray ? kvp.Value.array : kvp.Value.instance;
                            fields.Add(kvp.Key, ConvertToJsonValue(objectValue, visited));
                        }
                    }
                }

                visited.Remove(classInstance);
                return fields;
            }
            else
            {
                return value;
            }
        }

        private object HttpRequest(List<Expression> args, Scope callerScope)
        {
            object methodValue = evaluator.Evaluate(args[0], callerScope);
            if(methodValue is not string method)
            {
                throw runtime.Error("__http_request expected STR as first argument");
            }

            object urlValue = evaluator.Evaluate(args[1], callerScope);
            if (urlValue is not string url)
            {
                throw runtime.Error("__http_request expected STR as second argument");
            }

            method = method.ToUpperInvariant();

            HttpMethod httpMethod;
            switch (method)
            {
                case "GET":
                    httpMethod = HttpMethod.Get;
                    break;
                case "POST":
                    httpMethod = HttpMethod.Post;
                    break;
                default:
                    throw runtime.Error($"Unsupported HTTP method: {method}");
            }

            if (method == "GET")
            {
                if (args.Count != 2)
                {
                    throw runtime.Error("__http_request expected 2 arguments with method GET");
                }
                try
                {
                    using HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
                    using HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();

                    string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var header in response.Headers)
                    {
                        headers[header.Key] = string.Join(", ", header.Value);
                    }

                    foreach (var header in response.Content.Headers)
                    {
                        headers[header.Key] = string.Join(", ", header.Value);
                    }

                    int id = nextResId++;

                    httpResponses[id] = new NativeHttpResponse((int)response.StatusCode, body, headers);

                    return id;
                }
                catch (HttpRequestException)
                {
                    throw runtime.Error("HTTP request failed: network error");
                }
                catch (TaskCanceledException)
                {
                    throw runtime.Error("HTTP request failed: request timed out");
                }
                catch (UriFormatException)
                {
                    throw runtime.Error("HTTP request failed: invalid URL");
                }
            }
            else if (method == "POST")
            {
                if (args.Count != 4)
                {
                    throw runtime.Error("__http_request expected 4 arguments with method POST");
                }
                object bodyValue = evaluator.Evaluate(args[2], callerScope);
                object contentValue = evaluator.Evaluate(args[3], callerScope);
                if (bodyValue is not string body)
                {
                    throw runtime.Error("__http_request using POST method expects STR for third argument");
                }
                if (contentValue is not string contentType)
                {
                    throw runtime.Error("__http_request using POST method expects STR for fourth argument");
                }

                try
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(httpMethod, url))
                    {
                        request.Content = new StringContent(body, Encoding.UTF8, contentType);
                        using HttpResponseMessage response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                        string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var header in response.Headers)
                        {
                            headers[header.Key] = string.Join(", ", header.Value);
                        }

                        foreach (var header in response.Content.Headers)
                        {
                            headers[header.Key] = string.Join(", ", header.Value);
                        }

                        int id = nextResId++;

                        httpResponses[id] = new NativeHttpResponse((int)response.StatusCode, responseBody, headers);

                        return id;
                    }
                    ;
                }
                catch (HttpRequestException)
                {
                    throw runtime.Error("HTTP request failed: network error");
                }
                catch (TaskCanceledException)
                {
                    throw runtime.Error("HTTP request failed: request timed out");
                }
                catch (UriFormatException)
                {
                    throw runtime.Error("HTTP request failed: invalid URL");
                }
            }
            else
            {
                throw runtime.Error($"Unsupported HTTP method: {method}");
            }
        }

        private NativeHttpResponse GetHttpResponse(List<Expression> arguments, Scope callerScope, string functionName)
        {
            if (arguments.Count != 1)
            {
                throw runtime.Error($"{functionName} expected 1 argument");
            }

            object value = evaluator.Evaluate(arguments[0], callerScope);

            if (value is not int id)
            {
                throw runtime.Error($"{functionName} expected a HTTP response handle");
            }

            if (!httpResponses.TryGetValue(id, out NativeHttpResponse? response))
            {
                throw runtime.Error("Invalid or expired HTTP response handle");
            }

            return response;
        }

        private object HttpStatus(List<Expression> args, Scope callerScope)
        {
            NativeHttpResponse response = GetHttpResponse(args, callerScope, "__http_status");
            return response.statusCode;
        }

        private object HttpBody(List<Expression> args, Scope callerScope)
        {
            NativeHttpResponse response = GetHttpResponse(args, callerScope, "__http_body");
            return response.body;
        }

        private object HttpHeader(List<Expression> args, Scope callerScope)
        {
            if(args.Count != 2)
            {
                throw runtime.Error("__http_header expected 2 arguments");
            }

            object handleValue = evaluator.Evaluate(args[0], callerScope);
            object headerValue = evaluator.Evaluate(args[1], callerScope);

            if(handleValue is not int id)
            {
                throw runtime.Error("__http_header expected a HTTP response handle");
            }
            if(headerValue is not string headerName)
            {
                throw runtime.Error("__http_header expected STR as header name");
            }

            if(!httpResponses.TryGetValue(id, out NativeHttpResponse? response))
            {
                throw runtime.Error("Invalid or expired HTTP response handle");
            }

            if(!response.headers.TryGetValue(headerName, out string? value))
            {
                return "";
            }

            return value;
        }

        private object? HttpDispose(List<Expression> args, Scope callerScope)
        {
            if(args.Count != 1)
            {
                throw runtime.Error("__http_dispose expected 1 argument");
            }

            object value = evaluator.Evaluate(args[0], callerScope);

            if(value is not int id)
            {
                throw runtime.Error("__http_dispose expected a HTTP response handle");
            }

            httpResponses.Remove(id);
            return null;
        }
    }

    public class NativeHttpResponse
    {
        public int statusCode;
        public string body;
        public Dictionary<string, string> headers;

        public NativeHttpResponse(int statusCode, string body, Dictionary<string, string> headers)
        {
            this.statusCode = statusCode;
            this.body = body;
            this.headers = headers;
        }
    }
}
