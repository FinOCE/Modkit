﻿namespace Discordfs.Rest.Common

open Discordfs.Types.Utils
open System.Collections.Generic
open System.Net.Http
open System.Net.Http.Headers

[<AutoOpen>]
module Payload =
    type IPayloadBuilder =
        abstract member ToContent: unit -> HttpContent
    
    type JsonPayloadBuilder() =
        member val Properties: IDictionary<string, obj> = Dictionary()

        member this.Yield(_) =
            this

        [<CustomOperation "required">]
        member this.Required (_, name: string, value: 'a) =
            this.Properties.Add(name, value)
            this

        [<CustomOperation "optional">]
        member this.Optional (_, name: string, value: 'a option) =
            if value.IsSome then
                this.Properties.Add(name, value)
            this

        interface IPayloadBuilder with
            member this.ToContent () =
                new StringContent(FsJson.serialize this.Properties, MediaTypeHeaderValue("application/json"))

    let json = JsonPayloadBuilder()

    type JsonListPayload<'a>(list: 'a list) =
        interface IPayloadBuilder with
            member _.ToContent () =
                new StringContent(FsJson.serialize list, MediaTypeHeaderValue("application/json"))

    // TODO: Create payload types for different file contents

    type MultipartPayloadBuilder() =
        let mutable fileCount = 0

        member val Form = new MultipartFormDataContent()

        member this.Yield(_) =
            this

        [<CustomOperation "part">]
        member this.Part (_, name: string, content: unit -> IPayloadBuilder) =
            this.Form.Add(content().ToContent(), name)
            this
            
        [<CustomOperation "file">]
        member this.File (_, fileName: string, fileContent: unit -> IPayloadBuilder) =
            this.Form.Add(fileContent().ToContent(), $"files[{fileCount}]", fileName)
            fileCount <- fileCount + 1
            this

        interface IPayloadBuilder with
            member this.ToContent () =
                this.Form

    let multipart = MultipartPayloadBuilder()

    [<AbstractClass>]
    type Payload() =
        abstract member Content: IPayloadBuilder