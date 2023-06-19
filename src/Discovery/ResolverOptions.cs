#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using Microsoft.Extensions.Logging;

namespace Discovery;

/// <summary>
/// Options for creating a resolver.
/// <para>
/// Note: Experimental API that can change or be removed without any prior notice.
/// </para>
/// </summary>
public sealed class ResolverOptions
{
    // TODO: Options were previously internally created from GrpcChannelOptions. Need to rethink how this works byitself.
    // Does configuration come from Microsoft.Extensions.Configuration?
    private TimeSpan? _maxReconnectBackoff;
    private TimeSpan _initialReconnectBackoff;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolverOptions"/> class.
    /// </summary>
    public ResolverOptions(Uri address, int defaultPort, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        Address = address;
        DefaultPort = defaultPort;
        LoggerFactory = loggerFactory;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the address.
    /// </summary>
    public Uri Address { get; }

    /// <summary>
    /// Gets the default port. This port is used when the resolver address doesn't specify a port.
    /// </summary>
    public int DefaultPort { get; }

    /// <summary>
    /// Gets the logger factory.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets or sets the the maximum time between subsequent connection attempts.
    /// <para>
    /// The reconnect backoff starts at an initial backoff and then exponentially increases between attempts, up to the maximum reconnect backoff.
    /// Reconnect backoff adds a jitter to randomize the backoff. This is done to avoid spikes of connection attempts.
    /// </para>
    /// <para>
    /// A <c>null</c> value removes the maximum reconnect backoff limit. The default value is 120 seconds.
    /// </para>
    /// <para>
    /// Note: Experimental API that can change or be removed without any prior notice.
    /// </para>
    /// </summary>
    public TimeSpan? MaxReconnectBackoff
    {
        get => _maxReconnectBackoff;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException("Maximum reconnect backoff must be greater than zero.");
            }
            _maxReconnectBackoff = value;
        }
    }

    /// <summary>
    /// Gets or sets the time between the first and second connection attempts.
    /// <para>
    /// The reconnect backoff starts at an initial backoff and then exponentially increases between attempts, up to the maximum reconnect backoff.
    /// Reconnect backoff adds a jitter to randomize the backoff. This is done to avoid spikes of connection attempts.
    /// </para>
    /// <para>
    /// Defaults to 1 second.
    /// </para>
    /// <para>
    /// Note: Experimental API that can change or be removed without any prior notice.
    /// </para>
    /// </summary>
    public TimeSpan InitialReconnectBackoff
    {
        get => _initialReconnectBackoff;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentException("Initial reconnect backoff must be greater than zero.");
            }
            _initialReconnectBackoff = value;
        }
    }
}
