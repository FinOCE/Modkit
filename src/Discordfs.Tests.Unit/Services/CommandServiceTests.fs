﻿namespace Modkit.Discordfs.Services

open Microsoft.VisualStudio.TestTools.UnitTesting
open Modkit.Discordfs.Commands
open Modkit.Discordfs.Types
open System.Threading.Tasks

type SampleCommand () =
    inherit Command ()

    override _.Data = CreateGlobalApplicationCommand.build(
        Name = "sample",
        Description = "Sample command for testing the command service"
    )

    override _.Execute _ =
        Task.FromResult <| Ok (InteractionCallback.build(
            Type = InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE,
            Data = InteractionCallbackMessageData.buildBase(
                Content = "Hello world"
            )
        ))


[<TestClass>]
type CommandServiceTests () =
    let createMockInteraction (commandName: string option) =
        {
            Id = "";
            ApplicationId = "";
            Type = InteractionType.APPLICATION_COMMAND;
            Guild = None;
            GuildId = None;
            Channel = None;
            ChannelId = None;
            Member = None;
            User = None;
            Data =
                match commandName with
                | None -> None
                | Some name -> Some {
                    Id = "";
                    Name = name;
                    Type = ApplicationCommandType.MESSAGE;
                    Resolved = None;
                    Options = None;
                    GuildId = None;
                    TargetId = None;
                };
            Token = "";
            Version = 0;
            Message = None;
            Locale = None;
            GuildLocale = None;
            AuthorizingIntegrationOwners = Map<ApplicationIntegrationType, ApplicationIntegrationTypeConfiguration>([]);
            AppPermissions = "";
            Entitlements = [];
            Context = None;
        }
        
    [<TestMethod>]
    member _.getCommandName_GetsExistingName () =
        // Arrange
        let commandService = CommandService []

        let interaction = createMockInteraction (Some "sample")

        // Act
        let name = commandService.getCommandName interaction

        // Assert
        Assert.IsTrue(name.IsSome)
        Assert.AreEqual("sample", name.Value)


    [<TestMethod>]
    member _.getCommandName_DoesNotGetNonExistentName () =
        // Arrange
        let commandService = CommandService []

        let interaction = createMockInteraction None

        // Act
        let name = commandService.getCommandName interaction

        // Assert
        Assert.IsTrue(name.IsNone)

    [<TestMethod>]
    member _.getCommand_GetsExistingCommand () =
        // Arrange
        let commands: Command list = [SampleCommand()]
        let commandService = CommandService commands

        // Act
        let command = commandService.getCommand "sample"

        // Assert
        Assert.IsTrue(command.IsSome)
        Assert.AreEqual("sample", command.Value.Data.Name)

    [<TestMethod>]
    member _.getCommand_DoesNotGetNonExistentCommand () =
        // Arrange
        let commandService = CommandService []

        // Act
        let command = commandService.getCommand "sample"

        // Assert
        Assert.IsTrue(command.IsNone)

    [<TestMethod>]
    member _.Execute_RunsSuccessfulCommand (): Task = task {
        // Arrange
        let commands: Command list = [SampleCommand()]
        let commandService: ICommandService = CommandService commands

        let interaction = createMockInteraction (Some "sample")

        // Act
        let! res = commandService.Execute interaction

        // Assert
        match res with
        | Error err -> failwith err
        | Ok res -> Assert.AreEqual(InteractionCallbackType.CHANNEL_MESSAGE_WITH_SOURCE, res.Type)
    }

    [<TestMethod>]
    member _.Execute_ReturnsErrorOnUnknownCommand (): Task = task {
        // Arrange
        let commandService: ICommandService = CommandService []

        let interaction = createMockInteraction (Some "sample")

        // Act
        let! res = commandService.Execute interaction

        // Assert
        match res with
        | Ok _ -> failwith "Supposed to fail"
        | Error err -> Assert.AreEqual("Unknown command", err)
    }

    [<TestMethod>]
    member _.Execute_ReturnsErrorOnMissingCommandName (): Task = task {
        // Arrange
        let commandService: ICommandService = CommandService []

        let interaction = createMockInteraction None

        // Act
        let! res = commandService.Execute interaction

        // Assert
        match res with
        | Ok _ -> failwith "Supposed to fail"
        | Error err -> Assert.AreEqual("Missing command name in interaction data", err)
    }
