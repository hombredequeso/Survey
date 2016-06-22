namespace Survey

open Nancy

type IndexModule() as x =
    inherit NancyModule()
    do x.Get.["/"] <- fun _ -> box (x.Response.AsRedirect("/App/index.html"))

