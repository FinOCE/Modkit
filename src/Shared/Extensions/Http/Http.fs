﻿namespace System.Net.Http

open System
open System.Web

module Http =
    type RequestBuilder(host: string) =
        member val Host = host with get, set
        member val HttpRequestMessage = new HttpRequestMessage()

        member this.Yield(_) =
            this

        [<CustomOperation>]
        member this.host (_, host: string) =
            this.Host <- host

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.get (_, endpoint: string) =
            this.HttpRequestMessage.RequestUri <- new Uri(this.Host + "/" + endpoint)
            this.HttpRequestMessage.Method <- HttpMethod.Get

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.post (_, endpoint: string) =
            this.HttpRequestMessage.RequestUri <- new Uri(this.Host + "/" + endpoint)
            this.HttpRequestMessage.Method <- HttpMethod.Post

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.put (_, endpoint: string) =
            this.HttpRequestMessage.RequestUri <- new Uri(this.Host + "/" + endpoint)
            this.HttpRequestMessage.Method <- HttpMethod.Put

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.patch (_, endpoint: string) =
            this.HttpRequestMessage.RequestUri <- new Uri(this.Host + "/" + endpoint)
            this.HttpRequestMessage.Method <- HttpMethod.Patch

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.delete (_, endpoint: string) =
            this.HttpRequestMessage.RequestUri <- new Uri(this.Host + "/" + endpoint)
            this.HttpRequestMessage.Method <- HttpMethod.Delete

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.header (_, key: string, value: string) =
            this.HttpRequestMessage.Headers.Add(key, value)

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.header(_, key: string, value: string option) =
            match value with
            | Some value -> this.HttpRequestMessage.Headers.Add(key, value)
            | None -> ()

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.query(_, key: string, value: string) =
            let uriBuilder = UriBuilder(this.HttpRequestMessage.RequestUri)
            let query = HttpUtility.ParseQueryString(uriBuilder.Query)
            query.Add(key, value)
            uriBuilder.Query <- query.ToString()
            this.HttpRequestMessage.RequestUri <- uriBuilder.Uri

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.query(_, key: string, value: string option) =
            match value with
            | Some value ->
                let uriBuilder = UriBuilder(this.HttpRequestMessage.RequestUri)
                let query = HttpUtility.ParseQueryString(uriBuilder.Query)
                query.Add(key, value)
                uriBuilder.Query <- query.ToString()
                this.HttpRequestMessage.RequestUri <- uriBuilder.Uri
            | None -> ()

            this.HttpRequestMessage

        [<CustomOperation>]
        member this.payload(_, payload: Payload) =
            let content = payload.Content.ToContent()

            this.HttpRequestMessage.Headers.Add("Content-Type", content.Headers.ContentType.MediaType)
            this.HttpRequestMessage.Content <- content
            
            this.HttpRequestMessage
    
        // TODO: Fix this builder to not use `this.HttpRequestMessage`
