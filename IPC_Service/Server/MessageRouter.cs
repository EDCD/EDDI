#nullable enable

using EddiIPC_Service.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;

namespace EddiIPC_Service.Server;

/// <summary>
/// Routes incoming messages to registered handlers based on message type.
/// Supports event broadcasting and command dispatch patterns.
/// </summary>
public class MessageRouter
{
    private readonly Dictionary<string, List<Func<MessageEnvelope, ConnectionContext, Task>>> _handlers = [];
    private readonly object _lockObj = new();

    /// <summary>
    /// Register a handler for a specific message type.
    /// Handlers are called in registration order.
    /// </summary>
    public void RegisterHandler ( string messageType, Func<MessageEnvelope, ConnectionContext, Task> handler )
    {
        ArgumentNullException.ThrowIfNull( messageType );
        ArgumentNullException.ThrowIfNull( handler );

        lock ( _lockObj )
        {
            if ( !_handlers.TryGetValue( messageType, out var handlers ) )
            {
                handlers = [ ];
                _handlers[ messageType ] = handlers;
            }

            handlers.Add( handler );
        }

        Logging.Debug( $"Handler registered for message type: {messageType}" );
    }

    /// <summary>
    /// Unregister all handlers for a specific message type.
    /// </summary>
    public void UnregisterHandlers ( string messageType )
    {
        ArgumentNullException.ThrowIfNull( messageType );

        lock ( _lockObj )
        {
            _handlers.Remove( messageType );
        }

        Logging.Debug( $"Handlers unregistered for message type: {messageType}" );
    }

    /// <summary>
    /// Route a message to all registered handlers for its type.
    /// Handlers execute sequentially in registration order.
    /// If any handler throws, subsequent handlers still run but exception is logged.
    /// </summary>
    public async Task RouteAsync ( MessageEnvelope message, ConnectionContext context )
    {
        ArgumentNullException.ThrowIfNull( message );
        ArgumentNullException.ThrowIfNull( context );

        List<Func<MessageEnvelope, ConnectionContext, Task>>? handlersForType;

        lock ( _lockObj )
        {
            _handlers.TryGetValue( message.Type, out handlersForType );
        }

        if ( handlersForType == null || handlersForType.Count == 0 )
        {
            Logging.Warn( $"No handlers registered for message type: {message.Type} from session {context.SessionId}" );
            return;
        }

        foreach ( var handler in handlersForType )
        {
            try
            {
                await handler( message, context );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Handler error for {message.Type} from session {context.SessionId}: {ex.Message}", ex );
                // Continue to next handler even if this one fails
            }
        }
    }

    /// <summary>
    /// Check if there are any handlers registered for a message type.
    /// </summary>
    public bool HasHandlers ( string messageType )
    {
        ArgumentNullException.ThrowIfNull( messageType );

        lock ( _lockObj )
        {
            return _handlers.ContainsKey( messageType ) && _handlers[ messageType ].Count > 0;
        }
    }

    /// <summary>
    /// Get the number of handlers for a message type.
    /// </summary>
    public int GetHandlerCount ( string messageType )
    {
        ArgumentNullException.ThrowIfNull( messageType );

        lock ( _lockObj )
        {
            return _handlers.TryGetValue( messageType, out var handlers ) ? handlers.Count : 0;
        }
    }

    /// <summary>
    /// Clear all registered handlers.
    /// </summary>
    public void ClearAllHandlers ()
    {
        lock ( _lockObj )
        {
            _handlers.Clear();
        }

        Logging.Debug( "All message handlers cleared" );
    }
}
