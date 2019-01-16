﻿using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CosmosDbRepository.Implementation
{
    internal class CosmosDbRepositoryBuilder<T>
        : ICosmosDbRepositoryBuilder
    {
        private List<IncludedPath> _includePaths = new List<IncludedPath>();
        private List<ExcludedPath> _excludePaths = new List<ExcludedPath>();
        private IndexingMode _indexingMode = IndexingMode.Consistent;

        public string Id { get; private set; }

        public ICosmosDbRepositoryBuilder WithId(string id)
        {
            Id = id;
            return this;
        }

        public ICosmosDbRepositoryBuilder IncludeIndexPath(string path, params Index[] indexes)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid Include Path", nameof(path));
            };

            _includePaths.Add(new IncludedPath
            {
                Path = path,
                Indexes = (indexes?.Any() ?? false)
                    ? new Collection<Index>(indexes)
                    : null
            });

            return this;
        }

        public ICosmosDbRepositoryBuilder ExcludeIndexPath(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (paths.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Invalid Exclude Path", nameof(paths));
            }

            if (paths.Any())
            {
                _excludePaths.AddRange(paths.Select(path => new ExcludedPath { Path = path }));
            };

            return this;
        }

        public ICosmosDbRepository Build(IDocumentClient client, ICosmosDb documentDb)
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("Id not specified");

            var indexingPolicy = new IndexingPolicy
            {
                IndexingMode = _indexingMode
            };

            if (_includePaths.Any())
            {
                indexingPolicy.IncludedPaths = new Collection<IncludedPath>(_includePaths);
            }

            if (_excludePaths.Any())
            {
                indexingPolicy.ExcludedPaths = new Collection<ExcludedPath>(_excludePaths);
            }

            return new CosmosDbRepository<T>(client, documentDb, Id, indexingPolicy);
        }
    }
}