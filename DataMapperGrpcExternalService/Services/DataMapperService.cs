using Google.Protobuf;
using Grpc.Core;
using Peregrine.DataMapper.Api.Api;
using Peregrine.DataMapper.Core.Core;
using static Peregrine.DataMapper.Service.DataMapperServiceBase;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Peregrine.DataMapper.Api.Spi;
using Peregrine.DataMapper.Model;
using Peregrine.DataMapper.Model.Helpers;
using Perigrine.DataMapper.gRPC_Service.Model;

namespace DataMapperGrpcExternalService.Services
{
    public class DataMapperService : ExternalProcessor.ExternalProcessorBase
    {
        private readonly ILogger<DataMapperService> _logger;
        protected readonly DefaultDataMapperContextFactory dataMapperContextFactory;
        protected readonly IDataMapperContext defaultContext;
        protected readonly DataMapperMappingService dataMapperMappingService;
        protected static ConcurrentDictionary<string, DataMapperContextCache> CACHE_CONTEXT = new ConcurrentDictionary<string, DataMapperContextCache>(16, 1);
        private string targetMessage;
        private string RequestDataMap="";
        private string ResponseDataMap="";
        public DataMapperService(ILogger<DataMapperService> logger, IConfiguration configuration)
        {
            _logger = logger;

            var staticModules = DefaultStaticModuleProvider.StaticModules;
            if (staticModules == null)
            {
                List<IDataMapperModule> staticModulesList = new List<IDataMapperModule>();
                staticModulesList.Add(new Peregrine.DataMapper.XmlModule.Module.XmlModule());
                staticModulesList.Add(new Peregrine.DataMapper.JsonModule.Module.JsonModule());
                staticModulesList.Add(new Peregrine.DataMapper.CsvModule.Module.CsvModule());
                lock (CACHE_CONTEXT)
                {
                    DefaultStaticModuleProvider.StaticModules = staticModulesList;
                }
            }

            dataMapperContextFactory = DefaultDataMapperContextFactory.Instance;
            if (!dataMapperContextFactory.IsCustomActionLoaded)
            {
                dataMapperContextFactory.CheckAndLoadCustomFieldActions();
            }
            this.defaultContext = dataMapperContextFactory.CreateContext(new DataMapperMapping());
            if (configuration != null)
                dataMapperMappingService = new DataMapperMappingService(configuration);
        }

		public override async Task Process(IAsyncStreamReader<ProcessingRequest> requestStream, IServerStreamWriter<ProcessingResponse> responseStream, ServerCallContext context)
        {
			try
			{
				var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

				bool isProcessCompleted = false;
				while (!context.CancellationToken.IsCancellationRequested)
				{
					if (await requestStream.MoveNext())
					{
						Console.WriteLine("\n///////////Start//////////");


						// Process the current request
						var currentRequest = requestStream.Current;

						var response = new ProcessingResponse
						{
							// Set your response properties
						};
						response.ModeOverride = new ProcessingMode();


						switch (currentRequest.RequestCase)
						{
							case ProcessingRequest.RequestOneofCase.RequestHeaders:

								Console.WriteLine("\n\nActual Request Headers :- " + currentRequest.RequestHeaders);
                                
								foreach (var header in currentRequest.RequestHeaders.Headers.Headers)
                                {
                                    if (header != null && header.Key != null)
                                    {
										if(header.Key == "requestdatamappername")
											RequestDataMap = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(header.RawValue.ToBase64()));
										if(header.Key == "responsedatamappername")
											ResponseDataMap = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(header.RawValue.ToBase64()));
                                    }
                                }

								response.RequestHeaders = new HeadersResponse
								{
									Response = new CommonResponse
									{
										HeaderMutation = new HeaderMutation
										{

										}
									}
								};

								if (!string.IsNullOrEmpty(RequestDataMap))
                                {
                                    string requestReturnType = dataMapperMappingService.GetReturnType(RequestDataMap);

                                    var headers = new Google.Protobuf.Collections.RepeatedField<HeaderValueOption>();
                                    headers.Add(new HeaderValueOption
                                    {
                                        Header = new HeaderValue
                                        {
                                            Key = "content-type",
                                            RawValue = ByteString.CopyFromUtf8(requestReturnType)
                                        }

                                    });
                                    Console.WriteLine("\nRequest ReturnType :- " + requestReturnType);
                                    response.RequestHeaders.Response.HeaderMutation.SetHeaders.Add(headers);
                                }

								//if (currentRequest.RequestHeaders.EndOfStream)
								//{
								//	response.ModeOverride.RequestBodyMode = ProcessingMode.Types.BodySendMode.None;
								//	response.ModeOverride.ResponseHeaderMode = ProcessingMode.Types.HeaderSendMode.Send;
								//	response.ModeOverride.ResponseBodyMode = ProcessingMode.Types.BodySendMode.None;
								//}
								//else
								//{
								//	response.ModeOverride.RequestBodyMode = ProcessingMode.Types.BodySendMode.Buffered;
								//	response.ModeOverride.ResponseHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//	response.ModeOverride.ResponseBodyMode = ProcessingMode.Types.BodySendMode.None;
								//}

								break;

							case ProcessingRequest.RequestOneofCase.RequestBody:
                                Console.WriteLine("\n\nActual Request Body :- " + System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(currentRequest.RequestBody.Body.ToBase64())));
								response.RequestBody = new BodyResponse
								{
									Response = new CommonResponse
									{
										BodyMutation = new BodyMutation
										{
										}
									}
								};
								if (currentRequest.RequestBody.EndOfStream)
								{
                                    if (!string.IsNullOrEmpty(RequestDataMap))
                                    {
                                        DataMapperTransformModel transformModel = new DataMapperTransformModel();
                                        transformModel.DocumentName = RequestDataMap;
                                        transformModel.DocumentVersion = RequestDataMap;
                                        transformModel.MappingJson = dataMapperMappingService.GetMapping(RequestDataMap);
                                        transformModel.MessageData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(currentRequest.RequestBody.Body.ToBase64()));
                                        var output = await this.PerformTransform(transformModel);
                                        if (output != null && output.TargetDocuments != null)
                                        {
                                            targetMessage = output.TargetDocuments[0];
                                        }
                                        Console.WriteLine("\n\nChanged Request Body :- " + targetMessage);

                                        response.RequestBody.Response.BodyMutation.Body = ByteString.CopyFromUtf8(targetMessage);
                                    }
								}


								//response.ModeOverride.RequestHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//response.ModeOverride.ResponseHeaderMode = ProcessingMode.Types.HeaderSendMode.Send;
								//response.ModeOverride.ResponseBodyMode = ProcessingMode.Types.BodySendMode.None;

								break;

							case ProcessingRequest.RequestOneofCase.ResponseHeaders:
								Console.WriteLine("\n\nActual Response Headers :- " + currentRequest.ResponseHeaders);

								response.ResponseHeaders = new HeadersResponse
								{
									Response = new CommonResponse
									{
										HeaderMutation = new HeaderMutation
										{

										}
									}
								};

								if (!string.IsNullOrEmpty(ResponseDataMap))
                                {
                                    string responseReturnType = dataMapperMappingService.GetReturnType(ResponseDataMap);
                                    

                                    var headers = new Google.Protobuf.Collections.RepeatedField<HeaderValueOption>();
                                    headers.Add(new HeaderValueOption
                                    {
                                        Header = new HeaderValue
                                        {
                                            Key = "content-type",
                                            RawValue = ByteString.CopyFromUtf8(responseReturnType)
                                        }

                                    });
                                    Console.WriteLine("\nResponse ReturnType :- " + responseReturnType);
                                    response.ResponseHeaders.Response.HeaderMutation.SetHeaders.Add(headers);
                                }

        //                        if (currentRequest.ResponseHeaders.EndOfStream)
        //                        {
								//	response.ModeOverride.RequestHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//	response.ModeOverride.RequestBodyMode = ProcessingMode.Types.BodySendMode.None;
        //                            response.ModeOverride.ResponseBodyMode = ProcessingMode.Types.BodySendMode.None;

        //                            isProcessCompleted = true;

								//}
        //                        else
        //                        {
        //                            response.ModeOverride.RequestHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
        //                            response.ModeOverride.RequestBodyMode = ProcessingMode.Types.BodySendMode.None;
        //                            response.ModeOverride.ResponseBodyMode = ProcessingMode.Types.BodySendMode.Buffered;
        //                        }

								break;

							case ProcessingRequest.RequestOneofCase.ResponseBody:
                                Console.WriteLine("\n\nActual Response Body :- " + System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(currentRequest.ResponseBody.Body.ToBase64())));
								response.ResponseBody = new BodyResponse
                                {
                                    Response = new CommonResponse
                                    {
                                        BodyMutation = new BodyMutation
                                        {
                                        }
                                    }
                                };

                                if (currentRequest.ResponseBody.EndOfStream)
								{
                                    if (!string.IsNullOrEmpty(ResponseDataMap))
                                    {
                                        DataMapperTransformModel transformModel = new DataMapperTransformModel();
                                        transformModel.DocumentName = ResponseDataMap;
                                        transformModel.DocumentVersion = ResponseDataMap;
                                        transformModel.MappingJson = dataMapperMappingService.GetMapping(ResponseDataMap);
                                        transformModel.MessageData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(currentRequest.ResponseBody.Body.ToBase64()));
                                        var output = await this.PerformTransform(transformModel);
                                        if (output != null && output.TargetDocuments != null)
                                        {
                                            targetMessage = output.TargetDocuments[0];
                                        }
                                        Console.WriteLine("\n\nChanged Response Body :- " + targetMessage);
                                        response.ResponseBody.Response.BodyMutation.Body = ByteString.CopyFromUtf8(targetMessage);
                                    }

								}

								//response.ModeOverride.RequestHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//response.ModeOverride.RequestBodyMode = ProcessingMode.Types.BodySendMode.None;
								//response.ModeOverride.ResponseHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								isProcessCompleted = true;

								break;

							case ProcessingRequest.RequestOneofCase.RequestTrailers:
								Console.WriteLine("\n\nRequest Trailers :- " + currentRequest.RequestTrailers);

								//response.ModeOverride = new ProcessingMode();
								//response.ModeOverride.RequestHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//response.ModeOverride.RequestBodyMode = ProcessingMode.Types.BodySendMode.None;
								//response.ModeOverride.ResponseHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//response.ModeOverride.ResponseBodyMode = ProcessingMode.Types.BodySendMode.None;
								//response.ModeOverride.RequestTrailerMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//response.ModeOverride.ResponseTrailerMode = ProcessingMode.Types.HeaderSendMode.Send;
								break;

							case ProcessingRequest.RequestOneofCase.ResponseTrailers:
								Console.WriteLine("\n\nResponse Trailers :- " + currentRequest.ResponseTrailers);
								isProcessCompleted = true;

								response.ModeOverride.RequestHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								response.ModeOverride.RequestBodyMode = ProcessingMode.Types.BodySendMode.None;
								response.ModeOverride.ResponseHeaderMode = ProcessingMode.Types.HeaderSendMode.Skip;
								response.ModeOverride.ResponseBodyMode = ProcessingMode.Types.BodySendMode.None;
								//response.ModeOverride.RequestTrailerMode = ProcessingMode.Types.HeaderSendMode.Skip;
								//response.ModeOverride.ResponseTrailerMode = ProcessingMode.Types.HeaderSendMode.Skip;
								break;

							default:
								throw new Exception("requestStream type is not valid !");
						}

						await responseStream.WriteAsync(response);

        				Console.WriteLine("///////////End//////////");

						if (isProcessCompleted)
							break;
					}
					else
					{
						break;
					}
				}


			}
			catch (RpcException ex)
			{
				// Handle exceptions if necessary
			}
		}



        private async Task<DataMappingResultModel> PerformTransform(DataMapperTransformModel transformModel)
        {
            DataMappingResultModel mappingResult = new DataMappingResultModel();
            List<AuditInfo> auditInfoList = new List<AuditInfo>();
            mappingResult.Audits = auditInfoList;
            try
            {
                try
                {
                    if (transformModel == null)
                    {
                        throw new Exception("Error while transforming the message, transformModel = null");
                    }
                    if (transformModel.MessageData == null)
                    {
                        throw new Exception("Error while transforming the message, MessageData is null");
                    }
                    if (transformModel.MappingJson == null)
                    {
                        throw new Exception("Error while transforming the message, MappingJson is null");
                    }

                    IDataMapperContext context = null;
                    DataMapperMapping mapping = null;
                    DataMapperContextCache cacheEntry = null;

                    CACHE_CONTEXT.TryGetValue(transformModel.DocumentName, out cacheEntry);
                    if (cacheEntry != null)
                    {
                        //if (cacheEntry.Version == transformModel.DocumentVersion)
                        //{
                        //    mapping = cacheEntry.DataMapperMapping
                        //}
                    }
                    string actualString = null;
                    if (mapping == null)
                    {
                        actualString = transformModel.MappingJson;
                        //if (actualString.Contains("\"jsonType\""))
                        //    actualString = CoreHelper.ConvertMappingFileToDotNetCompatible(actualString);
                        mapping = JsonHelper.DeserializeJsonToObject<DataMapperMapping>(actualString, nameof(DataMapperMapping));
                        if (mapping == null)
                        {
                            throw new Exception("Error while transforming the message, DataMapperMapping = null");
                        }
                        context = dataMapperContextFactory.CreateContext(mapping);
                        cacheEntry = new DataMapperContextCache();
                        //cacheEntry.Version = transformModel.DocumentVersion;
                        //cacheEntry.DataMapperMapping = mapping;
                        long count = CACHE_CONTEXT.Count;//.mappingCount();
                        if (count > 40)
                        {
                            CACHE_CONTEXT.Clear();
                        }
                        if (CACHE_CONTEXT.ContainsKey(transformModel.DocumentName))
                        {
                            CACHE_CONTEXT[transformModel.DocumentName] = cacheEntry;
                        }
                        else
                        {
                            CACHE_CONTEXT.TryAdd(transformModel.DocumentName, cacheEntry);
                        }
                    }
                    else
                    {
                        context = dataMapperContextFactory.CreateContext(mapping);
                    }

                    IDataMapperSession session = context.CreateSession();
                    session.IsRuntime = true; // this is neuron runtime call, so indicate same.
                    //Set the session neuron properties and env vairables
                    if (transformModel.CustomProperties != null && transformModel.CustomProperties.Count > 0)
                    {
                        var soruceProperties = session.SourceProperties;
                        foreach (var customProperty in transformModel.CustomProperties)
                        {
                            soruceProperties.Add(customProperty.Key, customProperty.Value);
                        }
                    }
                    if (transformModel.NeuronEnvironment != null && transformModel.NeuronEnvironment.Count > 0)
                    {
                        var soruceVariables = session.SourceEnvironmentVariables;
                        foreach (var variable in transformModel.NeuronEnvironment)
                        {
                            soruceVariables.Add(variable.Key, variable.Value);
                        }
                    }
                    //session.setDefaultSourceDocument(getBody(transform));
                    var audits = session.Audits;

                    session.DefaultSourceDocument = transformModel.MessageData;
                    await context.Process(session);
                    object targetDoc = session.DefaultTargetDocument;
                    if (targetDoc != null)
                    {
                        List<String> targetDocs = new List<String>();
                        mappingResult.TargetDocuments = targetDocs;
                        targetDocs.Add(targetDoc.ToString());
                    }
                }
                catch (DataMapperErrorAuditException ex)
                {
                    Audit audit = ex.ErrorAudit;
                }
                catch (Exception ex1)
                {
                    //Audit audit = DataMapperUtil.CreateAudit(, null, null, null, null, ex1.ToString());
                    throw new Exception("Error while transforming the message: " + ex1.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while transforming the message: " + ex.ToString());
            }
            return mappingResult;
        }


    }
}