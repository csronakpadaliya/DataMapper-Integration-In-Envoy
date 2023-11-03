// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Protos/service.proto
// </auto-generated>
#pragma warning disable 0414, 1591, 8981
#region Designer generated code

using grpc = global::Grpc.Core;

namespace DataMapperGrpcExternalService {
  public static partial class ExternalProcessor
  {
    static readonly string __ServiceName = "envoy.service.ext_proc.v3.ExternalProcessor";

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::DataMapperGrpcExternalService.ProcessingRequest> __Marshaller_envoy_service_ext_proc_v3_ProcessingRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::DataMapperGrpcExternalService.ProcessingRequest.Parser));
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Marshaller<global::DataMapperGrpcExternalService.ProcessingResponse> __Marshaller_envoy_service_ext_proc_v3_ProcessingResponse = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::DataMapperGrpcExternalService.ProcessingResponse.Parser));

    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    static readonly grpc::Method<global::DataMapperGrpcExternalService.ProcessingRequest, global::DataMapperGrpcExternalService.ProcessingResponse> __Method_Process = new grpc::Method<global::DataMapperGrpcExternalService.ProcessingRequest, global::DataMapperGrpcExternalService.ProcessingResponse>(
        grpc::MethodType.DuplexStreaming,
        __ServiceName,
        "Process",
        __Marshaller_envoy_service_ext_proc_v3_ProcessingRequest,
        __Marshaller_envoy_service_ext_proc_v3_ProcessingResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::DataMapperGrpcExternalService.ServiceReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of ExternalProcessor</summary>
    [grpc::BindServiceMethod(typeof(ExternalProcessor), "BindService")]
    public abstract partial class ExternalProcessorBase
    {
      /// <summary>
      /// This begins the bidirectional stream that Envoy will use to
      /// give the server control over what the filter does. The actual
      /// protocol is described by the ProcessingRequest and ProcessingResponse
      /// messages below.
      /// </summary>
      /// <param name="requestStream">Used for reading requests from the client.</param>
      /// <param name="responseStream">Used for sending responses back to the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>A task indicating completion of the handler.</returns>
      [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
      public virtual global::System.Threading.Tasks.Task Process(grpc::IAsyncStreamReader<global::DataMapperGrpcExternalService.ProcessingRequest> requestStream, grpc::IServerStreamWriter<global::DataMapperGrpcExternalService.ProcessingResponse> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static grpc::ServerServiceDefinition BindService(ExternalProcessorBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Process, serviceImpl.Process).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("grpc_csharp_plugin", null)]
    public static void BindService(grpc::ServiceBinderBase serviceBinder, ExternalProcessorBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_Process, serviceImpl == null ? null : new grpc::DuplexStreamingServerMethod<global::DataMapperGrpcExternalService.ProcessingRequest, global::DataMapperGrpcExternalService.ProcessingResponse>(serviceImpl.Process));
    }

  }
}
#endregion
