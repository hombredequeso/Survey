namespace Survey

open System
open Nancy

type Question = {
    id: Guid
    section: int
    questionNumber: int
    text: string
}

type Link = {
    rel: string[]
    href: string
}

type SirenEntity<'T> = {
    klass: string[]
    properties: 'T
    links: Link[]
}

type SurveyModule() as x =
    inherit NancyModule()

    do x.Get.["/api/survey/start"] <- fun _ -> 
        let question = {
            id = System.Guid.NewGuid()
            section = 1
            questionNumber = 1
            text = "This is the very first question."
        }
        let selfLink = {
            rel = [|"self"|]
            href = sprintf "/api/survey/question/%s" (question.id.ToString())
        }
        let nextYesLink = {
            rel = [|"next-yes"|]
            href = sprintf "/api/survey/question/%s" (Guid.NewGuid().ToString())
        }
        let nextNoLink = {
            rel = [|"next-no"|]
            href = sprintf "/api/survey/question/%s" (Guid.NewGuid().ToString())
        }
        let entity: SirenEntity<Question> = {
            klass = [|"question"|]
            properties = question
            links = [|selfLink; nextYesLink; nextNoLink|]
        }
        box entity

