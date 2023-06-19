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

namespace Discovery;

public sealed class ResolverRegistry
{
    private readonly IEnumerable<ResolverFactory> _resolverFactories;

    public ResolverRegistry(IEnumerable<ResolverFactory> resolverFactories)
    {
        _resolverFactories = resolverFactories;
    }

    public Resolver? CreateResolver(string name, ResolverOptions options)
    {
        foreach (var resolverFactory in _resolverFactories)
        {
            if (resolverFactory.Name == name)
            {
                return resolverFactory.Create(options);
            }
        }

        return null;
    }
}
