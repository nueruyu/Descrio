using System;
using System.Collections.Generic;
using System.Linq;

namespace Descrio
{
    /// <summary>
    /// Represents a normalized, forward-slashed path for a module.
    /// </summary>
    public readonly struct ModulePath : IEquatable<ModulePath>
    {
        private readonly string _value;

        public ModulePath(string path)
        {
            _value = path?.Replace('\\', '/') ?? "";
        }

        /// <summary>
        /// Resolves a relative path against the current path.
        /// </summary>
        public ModulePath Resolve(string relativePath)
        {
            var targetPath = new ModulePath(relativePath);
            if (targetPath.IsAbsolute)
            {
                return targetPath;
            }

            var basePath = _value;

            var combinedPath = string.IsNullOrEmpty(basePath) || basePath == "/"
                ? $"/{relativePath}"
                : $"{basePath}/{relativePath}";

            var parts = combinedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var stack = new Stack<string>();

            foreach (var part in parts)
            {
                if (part == ".")
                {
                    continue;
                }
                if (part == "..")
                {
                    if (stack.Count > 0)
                    {
                        stack.Pop();
                    }
                }
                else
                {
                    stack.Push(part);
                }
            }
            return new ModulePath("/" + string.Join("/", stack.Reverse()));
        }

        /// <summary>
        /// Gets the directory part of the path.
        /// </summary>
        public ModulePath GetDirectoryPath()
        {
            var path = _value;
            if (path.Length > 1 && path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1);
            }

            var lastSlash = path.LastIndexOf('/');
            if (lastSlash <= 0)
            {
                return new ModulePath("/");
            }
            return new ModulePath(path.Substring(0, lastSlash));
        }

        public bool IsAbsolute => !string.IsNullOrEmpty(_value) && _value.StartsWith("/");

        public override string ToString() => _value ?? "";

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(_value ?? "");

        public override bool Equals(object obj) => obj is ModulePath other && Equals(other);

        public bool Equals(ModulePath other) => StringComparer.OrdinalIgnoreCase.Equals(_value, other._value);

        public static bool operator ==(ModulePath left, ModulePath right) => left.Equals(right);

        public static bool operator !=(ModulePath left, ModulePath right) => !(left == right);
    }
}