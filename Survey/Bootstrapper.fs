namespace Survey

open Nancy
open Nancy.Conventions
open Nancy.TinyIoc

type Bootstrapper() =
    inherit DefaultNancyBootstrapper()
    // The bootstrapper enables you to reconfigure the composition of the framework,
    // by overriding the various methods and properties.
    // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

    override this.ConfigureConventions (conventions) =
        base.ConfigureConventions(conventions)
        conventions.StaticContentsConventions.AddDirectory("App", "App")
        conventions.StaticContentsConventions.AddDirectory("app", "App")
