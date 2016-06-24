namespace SurveyTests

open NUnit.Framework

[<TestFixture>]
module SurveyDomainTests =

    open System
    open FsUnit

    [<Test>]
    let ``Basic Test`` () =
        1 |> should equal 1
