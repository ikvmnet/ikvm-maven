# IKVM.Sdk.Maven - IKVM support for Maven dependencies

IKVM.Sdk.Maven is a set of MSBuild extensions for referencing Maven artifacts within .NET SDK projects.

To use, install the IKVM.Sdk.Maven package from NuGet, and add a `<MavenReference />` element to your SDK-style project. Various Maven options are supported through item-metadata.

The simplest use case is to specify the group ID and artifact ID coordinates within item specificationoption, and use Version for the metadata.

```
<ItemGroup>
    <MavenReference Include="org.foo.bar:foo-lib" Version="1.2.3" />
</ItemGroup>
```

Optionally, use an arbitrary value for the item specification, and explicitely specify information through metadata:

```
<ItemGroup>
    <MavenReference Include="foo-lib">
      <GroupId>org.foo.bar</GroupId>
      <ArtifactId>foo-lib</ArtifactId>
      <Classifier></Classifier>
      <Version>1.2.3</Version>
    </MavenReference>
</ItemGroup>
```

## Transitive Dependencies

The `<MavenReference />` item group operates similar to a `dependency` in Maven. All transitive dependencies are
collected and resolved, and then the final output is produced. However, unlike PackageReferences, MavenReferences
are collected by the final output project, and reassessed. That is, each dependent Project within your .NET
SDK-style solution contributes its MavenReferences to project(s) which include it, and each project makes its own
dependency graph. Projects do not contribute their final built assemblies up. They only contribute their dependencies.
Allowing each project in a complicated solution to make its own local conflict resolution attempt.

PackageReferences are supported in the same way. Projects which include MavenReferences do not pack the generated IKVM
assemblies into their NuGet packages. Instead, they pack a partial POM file which only declares their dependencies. At 
build-time on the consumer's machine the final dependency graph is collected and generation happens. No generated
assemblies are published to NuGet. This prevents possible conflicts between NuGet packages and incompatible base Java
dependencies. For instance, if a package on nuget.org contained an actual copy of commons-logging.dll, there would be
no guarentee that this assembly was generated with the correct options to support a different package on nuget.org that
also depended on commons-logging. Since the final build machine is responsible for gathering and generating the
dependencies, these conflicts become simple Maven conflicts: multiple packages dependending on differnet versions of
the same thing, and Maven being unable to come up with a solution. Basically, not our problem.

MavenReferences fully support TFMs. A `<MavenReference />` element can be conditional based on TFM. As the partial
packaged POM-file is indexed by TFM in the generated .nupkg.
