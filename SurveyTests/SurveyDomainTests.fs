namespace SurveyTests

open NUnit.Framework
open System
open FsUnit

[<TestFixture>]
type SurveyDomainTests() =


    [<Test>]
    member public x.``Basic Test`` () =
        1 |> should equal 1
