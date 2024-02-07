using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Specification.Source;
using Hl7.FhirPath;
using Task = System.Threading.Tasks.Task;

namespace FhirPathsDemo;

internal static class FhirPathsDemo
{
    private static async Task Main()
    {
        var ehrFhirApiClient = new EhrFhirApiClient();
        
        // Some pages worth reading as to what is available on FHIR Path with the C# SDK
        // https://docs.fire.ly/projects/Firely-NET-SDK/en/stable/fhirpath/introduction.html
        // https://docs.fire.ly/projects/Firely-NET-SDK/en/stable/fhirpath/dialects.html
        // We have installed the Hl7.Fhir.STU3 package to the project to bring in the SDK

        // To call the FHIR API and get an extended condition we need to provide the RootInstanceId
        // whereas you will only have a SetGuid in the correlation table, we do eventually get
        // a RootInstanceGuid but that is provided by the round-tripping projector and we don't
        // want to have to rely on that. I think it is likely we can perform some sort of db query
        // passing in RunIds to get us back out root instances but I will do some more thinking on this.
        var patientId = "a76c5377-9201-eb11-80ce-0025b5011bcc";
        var rootInstanceId = "9679384e-7fff-466a-94f9-f311949f3280";

        // Initialise a summary provider which provides typing information from a FHIR specification
        // You should only load this once at the start of a process because it is computationally expensive
        // Perhaps register it with your DI as a singleton
        var zipSource = new ZipSource("specification.zip");
        var cachedResolver = new CachedResolver(zipSource);
        var summaryProvider = new StructureDefinitionSummaryProvider(cachedResolver);

        // This might not be required but it gives us access to the extra methods as defined in
        // https://docs.fire.ly/projects/Firely-NET-SDK/en/stable/fhirpath/dialects.html
        FhirPathCompiler.DefaultSymbolTable.AddFhirExtensions();

        // SourceNode is a tree navigator of the FHIR with no typing info
        var sourceNode = await ehrFhirApiClient.GetExtendedCondition(rootInstanceId, patientId);

        // We use the summary provider to provide typing information to the source node data
        var typedElement = sourceNode.ToTypedElement(summaryProvider);

        // Calling scalar allows us to select single values where we expect them, they are returned
        // as objects which we can then cast as we need
        var conditionCodePath = "code.coding.where(system = 'http://snomed.info/sct').code";
        var conditionCode = typedElement.Scalar(conditionCodePath);

        // We can use where clauses to filter
        var morphologyPath = "contained.QuestionnaireResponse.item.where(linkId = 'morphology').answer.value.code";
        var morphology = typedElement.Scalar(morphologyPath);

        // Use a select statement when we expect to access a collection
        var practitionerIdentifierPath = "contained.Practitioner.identifier";
        var practitionerIdentifiers = typedElement.Select(practitionerIdentifierPath);

        Console.WriteLine($"ConditionCode: {conditionCode}");
        Console.WriteLine($"Morphology: {morphology}");
        Console.WriteLine(string.Empty);

        Console.WriteLine("Practitioner Identifiers");
        Console.WriteLine("---------------------------------------------------------------");

        // Iterating a collection of identifiers xto get their associated values
        foreach (var identifier in practitionerIdentifiers)
        {
            Console.WriteLine($"System: {identifier.Scalar("system")}");
            Console.WriteLine($"Value: {identifier.Scalar("value")}");
        }

        // You can call ToPoco<T> to cast to an appropriate type should you wish.
        var condition = typedElement.ToPoco<Condition>();

        // It does make the accessing of nested properties a little uglier
        if (condition.Asserter != null)
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("Asserter Identifiers");
            Console.WriteLine("---------------------------------------------------------------");

            foreach (var asserterIdentifier in condition.Asserter.Identifier)
                Console.WriteLine($"{asserterIdentifier.Key}: {asserterIdentifier.Value}");
        }
    }
}