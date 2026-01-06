using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace mark.davison.rome.api;

// TODO: To base class
public static class PendingModelChangesChecker
{
    public static bool HasPendingModelChanges(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var serviceProvider = context.GetInfrastructure();
            var migrationsAssembly = serviceProvider.GetRequiredService<IMigrationsAssembly>();
            var migrationsModelDiffer = serviceProvider.GetRequiredService<IMigrationsModelDiffer>();
            var modelRuntimeInitializer = serviceProvider.GetRequiredService<IModelRuntimeInitializer>();

            // Get the snapshot model (from the last migration's ModelSnapshot)
            var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;
            if (snapshotModel == null)
            {
                // No migrations exist yet - check if the current model has any entities
                // Use runtime model (design-time model not available at runtime)
                return context.Model.GetEntityTypes().Any();
            }

            // Initialize the snapshot model with runtime dependencies before getting its relational model
            // The snapshot model is already finalized, but needs runtime initialization
            var initializedSnapshotModel = modelRuntimeInitializer.Initialize(snapshotModel, validationLogger: null);

            // Get the current model from the design-time model if available (for full configuration),
            // otherwise use runtime model. Note: IDesignTimeModel is only available at design-time.
            // At runtime, we use context.Model which may not have all configuration details.
            IModel currentModel;
            try
            {
                // Try to get design-time model (only available at design-time)
                var designTimeModelService = context.GetService<IDesignTimeModel>();
                currentModel = designTimeModelService?.Model ?? context.Model;
            }
            catch
            {
                // Fallback to runtime model if design-time model is not available
                currentModel = context.Model;
            }

            // Compare the snapshot model with the current model
            // The HasDifferences method compares the relational models
            var snapshotRelationalModel = initializedSnapshotModel.GetRelationalModel();
            var currentRelationalModel = currentModel.GetRelationalModel();

            return migrationsModelDiffer.HasDifferences(snapshotRelationalModel, currentRelationalModel);
        }
        catch (Exception ex)
        {
            // If we can't determine, throw to indicate the issue
            throw new InvalidOperationException(
                "Unable to check for pending model changes. Ensure migrations are properly configured.", ex);
        }
    }

    /// <summary>
    /// Gets detailed information about pending model changes.
    /// This method returns a description of what differences exist between the snapshot and current model.
    /// </summary>
    /// <param name="context">The DbContext instance</param>
    /// <returns>A string describing the differences, or null if no differences</returns>
    public static string GetPendingModelChanges(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var serviceProvider = context.GetInfrastructure();
            var migrationsAssembly = serviceProvider.GetRequiredService<IMigrationsAssembly>();
            var migrationsModelDiffer = serviceProvider.GetRequiredService<IMigrationsModelDiffer>();
            var modelRuntimeInitializer = serviceProvider.GetRequiredService<IModelRuntimeInitializer>();

            var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;
            if (snapshotModel == null)
            {
                // Use runtime model (design-time model not available at runtime)
                var entityCount = context.Model.GetEntityTypes().Count();
                return entityCount > 0
                    ? $"No migrations exist. The current model has {entityCount} entity type(s) that need to be migrated."
                    : "No migrations exist and the model is empty.";
            }

            // Initialize the snapshot model with runtime dependencies before getting its relational model
            // The snapshot model is already finalized, but needs runtime initialization
            var initializedSnapshotModel = modelRuntimeInitializer.Initialize(snapshotModel, validationLogger: null);
            var snapshotRelationalModel = initializedSnapshotModel.GetRelationalModel();

            // Get the current model from the design-time model if available (for full configuration),
            // otherwise use runtime model. Note: IDesignTimeModel is only available at design-time.
            // At runtime, we use context.Model which may not have all configuration details.
            IModel currentModel;
            try
            {
                // Try to get design-time model (only available at design-time)
                var designTimeModelService = context.GetService<IDesignTimeModel>();
                currentModel = designTimeModelService?.Model ?? context.Model;
            }
            catch
            {
                // Fallback to runtime model if design-time model is not available
                currentModel = context.Model;
            }
            var currentRelationalModel = currentModel.GetRelationalModel();

            var differences = migrationsModelDiffer.GetDifferences(snapshotRelationalModel, currentRelationalModel);

            if (differences.Count == 0)
            {
                return string.Empty;
            }

            var result = $"Found {differences.Count} pending model change(s) in DbContext:\n";
            foreach (var difference in differences)
            {
                result += $"  - {difference}\n";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"Error checking for pending model changes: {ex.Message}\nStack trace: {ex.StackTrace}";
        }
    }
}