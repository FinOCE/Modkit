﻿namespace Modkit.Roles.Application.Commands

open System.Net.Http

open Discordfs
open Discordfs.Rest.Modules
open MediatR

open Modkit.Roles.Domain.Entities

open Modkit.Roles.Application.Repositories

type CreateApplicationCommandError =
    | InvalidToken
    | MissingRedirectUri of string
    | InvalidClientSecret
    | DiscordAppUpdateFailed
    | DatabaseUpdateFailed

type CreateApplicationCommandResponse = Result<Application, CreateApplicationCommandError>

type CreateApplicationCommand (
    token: string,
    clientSecret: string,
    hostAuthority: string
) =
    interface IRequest<CreateApplicationCommandResponse>

    member val Token = token with get, set
    member val ClientSecret = clientSecret with get, set
    member val HostAuthority = hostAuthority with get, set

type CreateApplicationCommandHandler (
    applicationRepository: IApplicationRepository,
    httpClientFactory: IHttpClientFactory
) =
    interface IRequestHandler<CreateApplicationCommand, CreateApplicationCommandResponse> with
        member _.Handle (req, ct) = task {        
            let client = httpClientFactory.CreateClient() |> HttpClient.toBotClient req.Token

            let! currentApplication = client |> Bot.getApplication
            match currentApplication with
            | None ->
                return Error InvalidToken

            | Some app when Option.map (List.exists (fun v -> v = "")) app.RedirectUris |> Option.defaultValue false ->
                return Error (MissingRedirectUri $"{req.HostAuthority}/applications/{app.Id}/linked-role")

            | Some app ->
                let basicClient = httpClientFactory.CreateClient() |> HttpClient.toBasicClient app.Id req.ClientSecret
                let! clientCredentialsGrant = basicClient |> OAuthFlow.clientCredentialsGrant [Discordfs.Types.OAuth2Scope.IDENTIFY]

                match clientCredentialsGrant with
                | None -> return Error InvalidClientSecret
                | Some _ ->
                    let! appResult = applicationRepository.Put {
                        Id = app.Id
                        Token = req.Token
                        PublicKey = app.VerifyKey
                        ClientSecret = req.ClientSecret
                        Metadata = []
                    }

                    match appResult with
                    | Error _ -> return Error DatabaseUpdateFailed
                    | Ok application ->
                        let client = httpClientFactory.CreateClient() |> HttpClient.toBotClient req.Token

                        let! res = client |> Bot.editApplication [
                            Description "A custom bot built with Modkit Roles! https://modkit.org/linked-roles"
                            RoleConnectionVerificationUrl $"{req.HostAuthority}/applications/{app.Id}/linked-role"
                            InteractionsEndpointUrl $"{req.HostAuthority}/applications/{app.Id}/interactions"
                        ]

                        return res |> function | None -> Error DiscordAppUpdateFailed | Some _ -> Ok application

            // TODO: Rewrite into ROP
        }
