// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Xml.Linq;

namespace Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel
{
    internal unsafe static class SecretExtensions
    {
        /// <summary>
        /// Converts an <see cref="ISecret"/> to an &lt;masterKey&gt; element which is marked
        /// as requiring encryption.
        /// </summary>
        /// <returns></returns>
        public static XElement ToMasterKeyElement(this ISecret secret)
        {
            // Technically we'll be keeping the unprotected secret around in memory as
            // a string, so it can get moved by the GC, but we should be good citizens
            // and try to pin / clear our our temporary buffers regardless.
            byte[] unprotectedSecretRawBytes = new byte[secret.Length];
            string unprotectedSecretAsBase64String;
            fixed (byte* __unused__ = unprotectedSecretRawBytes)
            {
                try
                {
                    secret.WriteSecretIntoBuffer(new ArraySegment<byte>(unprotectedSecretRawBytes));
                    unprotectedSecretAsBase64String = Convert.ToBase64String(unprotectedSecretRawBytes);
                }
                finally
                {
                    Array.Clear(unprotectedSecretRawBytes, 0, unprotectedSecretRawBytes.Length);
                }
            }

            var masterKeyElement = new XElement("masterKey",
                new XComment(" Warning: the key below is in an unencrypted form. "),
                new XElement("value", unprotectedSecretAsBase64String));
            masterKeyElement.MarkAsRequiresEncryption();
            return masterKeyElement;
        }

        /// <summary>
        /// Converts a base64-encoded string into an <see cref="ISecret"/>.
        /// </summary>
        /// <returns></returns>
        public static Secret ToSecret(this string base64String)
        {
            byte[] unprotectedSecret = Convert.FromBase64String(base64String);
            fixed (byte* __unused__ = unprotectedSecret)
            {
                try
                {
                    return new Secret(unprotectedSecret);
                }
                finally
                {
                    Array.Clear(unprotectedSecret, 0, unprotectedSecret.Length);
                }
            }
        }
    }
}
