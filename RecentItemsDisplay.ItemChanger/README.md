# RecentItemsDisplay.ItemChanger

Addon for RecentItemsDisplay to enable showing items from ItemChanger.

## For developers

To control how an item is displayed, you may add IInteropTags with a message of RecentItemsDisplay
to an item. This will typically be done either when the item is added to Finder, retrieved from Finder
or during the OnGive[Global] event (it must be done prior to AfterGiveGlobal).

For a list of properties, you may review the ReadTags function in the RecentItemsModule.
