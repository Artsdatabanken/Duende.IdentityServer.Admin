﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using Skoruba.Duende.IdentityServer.Shared.Configuration.Configuration.Common;
using Skoruba.Duende.IdentityServer.Shared.Configuration.Configuration.Email;
using Skoruba.Duende.IdentityServer.Shared.Configuration.Email;

namespace Skoruba.Duende.IdentityServer.Shared.Configuration.Helpers
{
    public static class StartupHelpers
    {
        /// <summary>
        /// Add email senders - configuration of sendgrid, smtp senders
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddEmailSenders(this IServiceCollection services, IConfiguration configuration)
        {
            var smtpConfiguration = configuration.GetSection(nameof(SmtpConfiguration)).Get<SmtpConfiguration>();
            var sendGridConfiguration = configuration.GetSection(nameof(SendGridConfiguration)).Get<SendGridConfiguration>();

            if (sendGridConfiguration != null && !string.IsNullOrWhiteSpace(sendGridConfiguration.ApiKey))
            {
                services.AddSingleton<ISendGridClient>(_ => new SendGridClient(sendGridConfiguration.ApiKey));
                services.AddSingleton(sendGridConfiguration);
                services.AddTransient<IEmailSender, SendGridEmailSender>();
            }
            else if (smtpConfiguration != null && !string.IsNullOrWhiteSpace(smtpConfiguration.Host))
            {
                services.AddSingleton(smtpConfiguration);
                services.AddTransient<IEmailSender, SmtpEmailSender>();
            }
            else
            {
                services.AddSingleton<IEmailSender, LogEmailSender>();
            }
        }

        public static void AddDataProtection<TDbContext>(this IServiceCollection services, IConfiguration configuration)
                    where TDbContext : DbContext, IDataProtectionKeyContext
        {
            AddDataProtection<TDbContext>(
                services,
                configuration.GetSection(nameof(DataProtectionConfiguration)).Get<DataProtectionConfiguration>(),
                configuration.GetSection(nameof(AzureKeyVaultConfiguration)).Get<AzureKeyVaultConfiguration>());
        }

        public static void AddDataProtection<TDbContext>(this IServiceCollection services, DataProtectionConfiguration dataProtectionConfiguration, AzureKeyVaultConfiguration azureKeyVaultConfiguration)
            where TDbContext : DbContext, IDataProtectionKeyContext
        {
            var dataProtectionBuilder = services.AddDataProtection()
                .SetApplicationName("Skoruba.Duende.IdentityServer")
                .PersistKeysToDbContext<TDbContext>();

            if (dataProtectionConfiguration.ProtectKeysWithAzureKeyVault)
            {
                if (azureKeyVaultConfiguration.UseClientCredentials)
                {
                    dataProtectionBuilder.ProtectKeysWithAzureKeyVault(
                        new Uri(azureKeyVaultConfiguration.DataProtectionKeyIdentifier),
                        new ClientSecretCredential(azureKeyVaultConfiguration.TenantId,
                            azureKeyVaultConfiguration.ClientId, azureKeyVaultConfiguration.ClientSecret));
                }
                else
                {
                    dataProtectionBuilder.ProtectKeysWithAzureKeyVault(new Uri(azureKeyVaultConfiguration.DataProtectionKeyIdentifier), new DefaultAzureCredential());
                }
            }
        }

        public static void AddAzureKeyVaultConfiguration(this IConfiguration configuration, IConfigurationBuilder configurationBuilder)
        {
            if (!configuration.GetSection(nameof(AzureKeyVaultConfiguration)).Exists()) return;
            var azureKeyVaultConfiguration = configuration.GetSection(nameof(AzureKeyVaultConfiguration)).Get<AzureKeyVaultConfiguration>();

            if (!azureKeyVaultConfiguration.ReadConfigurationFromKeyVault) return;
            if (azureKeyVaultConfiguration.UseClientCredentials)
            {
                // Check environment variables if client secret is not provided
                if (string.IsNullOrWhiteSpace(azureKeyVaultConfiguration.ClientSecret))
                {
                    var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
                    // Throw an exception if client secret is not provided
                    if (string.IsNullOrWhiteSpace(clientSecret))
                    {
                        throw new Exception("Azure Key Vault Client Secret is not provided. Please provide it in secrets.json or in environment variables.");
                    }
                    azureKeyVaultConfiguration.ClientSecret = clientSecret;
                }
                configurationBuilder.AddAzureKeyVault(azureKeyVaultConfiguration.AzureKeyVaultEndpoint,
                    azureKeyVaultConfiguration.ClientId, azureKeyVaultConfiguration.ClientSecret);
            }
            else
            {
                var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                        .KeyVaultTokenCallback));

                configurationBuilder.AddAzureKeyVault(azureKeyVaultConfiguration.AzureKeyVaultEndpoint,
                    keyVaultClient, new DefaultKeyVaultSecretManager());
            }
        }
    }
}
