﻿namespace flack
    open System
    open System.Net.Sockets
    open System.Collections.Concurrent

    type BocketPool( number, size, callback) as this =
        let number = number
        let size = size
        let totalsize = (number * size)
        let buffer = Array.create totalsize 0uy
        let pool = new BlockingCollection<SocketAsyncEventArgs>(number:int)
        let mutable disposed = false
        let test = Observable.add callback
        let cleanUp() = 
            if not disposed then
                disposed <- true
                pool.CompleteAdding()
                while pool.Count > 1 do
                    pool.Take()
                        .Dispose()
        do
            let rec loop n =
                match n with
                | x when x < totalsize ->
                    let saea = new SocketAsyncEventArgs()
                    saea.Completed |> Observable.add callback
                    saea.SetBuffer(buffer, n, size)
                    this.CheckIn(saea)
                    loop (n + size)
                | _ -> ()
            loop 0                    
        member this.CheckOut() =
            pool.Take()
        member this.CheckIn(saea) =
            pool.Add(saea)
        member this.Count =
            pool.Count
        interface IDisposable with
            member this.Dispose() = cleanUp()