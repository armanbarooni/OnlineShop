        private async Task ProcessCategoriesAsync(List<ProductCategoryModel>? categories, CancellationToken cancellationToken)
        {
            if (categories == null || !categories.Any())
            {
                _logger.LogInformation("No categories to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} categories from Mahak", categories.Count);
            
            int created = 0;
            int updated = 0;
            int errors = 0;

            foreach (var mahakCategory in categories)
            {
                try
                {
                    if (mahakCategory.Deleted)
                    {
                        _logger.LogDebug("Skipping deleted category: {CategoryId}", mahakCategory.ProductCategoryId);
                        continue;
                    }

                    var mapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "ProductCategory", 
                        mahakCategory.ProductCategoryId, 
                        cancellationToken);

                    ProductCategory? existingCategory = null;
                    
                    if (mapping != null)
                    {
                        existingCategory = await _productCategoryRepository.GetByIdAsync(
                            mapping.LocalEntityId, 
                            cancellationToken);
                    }

                    if (existingCategory != null)
                    {
                        _logger.LogDebug("Updating category: {Name} (MahakId: {MahakId})", 
                            mahakCategory.Name, mahakCategory.ProductCategoryId);

                        existingCategory.SetName(mahakCategory.Name ?? "");
                        existingCategory.SetDescription(mahakCategory.Name ?? "");
                        
                        await _productCategoryRepository.UpdateAsync(existingCategory, cancellationToken);
                        updated++;
                    }
                    else
                    {
                        _logger.LogDebug("Creating new category: {Name} (MahakId: {MahakId})", 
                            mahakCategory.Name, mahakCategory.ProductCategoryId);

                        var newCategory = ProductCategory.Create(
                            name: mahakCategory.Name ?? "Unknown Category",
                            description: mahakCategory.Name ?? ""
                        );

                        await _productCategoryRepository.AddAsync(newCategory, cancellationToken);

                        var newMapping = MahakMapping.Create(
                            entityType: "ProductCategory",
                            localEntityId: newCategory.Id,
                            mahakEntityId: mahakCategory.ProductCategoryId,
                            mahakEntityCode: mahakCategory.ProductCategoryCode.ToString(),
                            notes: $"Synced from Mahak on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                        );

                        await _mahakMappingRepository.AddAsync(newMapping, cancellationToken);
                        created++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing category {CategoryId}: {Error}", 
                        mahakCategory.ProductCategoryId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "Category sync completed: {Created} created, {Updated} updated, {Errors} errors", 
                created, updated, errors);
        }

        private async Task ProcessProductDetailsAsync(List<ProductDetailModel>? productDetails, CancellationToken cancellationToken)
        {
            if (productDetails == null || !productDetails.Any())
            {
                _logger.LogInformation("No product details to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} product details from Mahak", productDetails.Count);
            
            int updated = 0;
            int errors = 0;

            foreach (var mahakDetail in productDetails)
            {
                try
                {
                    if (mahakDetail.Deleted)
                    {
                        _logger.LogDebug("Skipping deleted product detail: {DetailId}", mahakDetail.ProductDetailId);
                        continue;
                    }

                    // Find the product by MahakId
                    var productMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Product", 
                        mahakDetail.ProductId, 
                        cancellationToken);

                    if (productMapping == null)
                    {
                        _logger.LogWarning("Product not found for ProductDetail {DetailId}, ProductId: {ProductId}", 
                            mahakDetail.ProductDetailId, mahakDetail.ProductId);
                        continue;
                    }

                    var product = await _productRepository.GetByIdAsync(productMapping.LocalEntityId, cancellationToken);
                    
                    if (product == null)
                    {
                        _logger.LogWarning("Product entity not found for mapping {MappingId}", productMapping.Id);
                        continue;
                    }

                    // Update product price based on DefaultSellPriceLevel
                    decimal price = mahakDetail.DefaultSellPriceLevel switch
                    {
                        1 => mahakDetail.Price1,
                        2 => mahakDetail.Price2,
                        3 => mahakDetail.Price3,
                        4 => mahakDetail.Price4,
                        5 => mahakDetail.Price5,
                        _ => mahakDetail.Price1
                    };

                    if (price > 0)
                    {
                        product.SetPrice(price);
                        _logger.LogDebug("Updated price for product {ProductId}: {Price}", 
                            mahakDetail.ProductId, price);
                    }

                    // Update barcode if available
                    if (!string.IsNullOrEmpty(mahakDetail.Barcode))
                    {
                        product.SetBarcode(mahakDetail.Barcode);
                    }

                    await _productRepository.UpdateAsync(product, cancellationToken);
                    updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing product detail {DetailId}: {Error}", 
                        mahakDetail.ProductDetailId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "ProductDetail sync completed: {Updated} updated, {Errors} errors", 
                updated, errors);
        }

        private async Task ProcessInventoryAsync(List<ProductDetailStoreAssetModel>? inventory, CancellationToken cancellationToken)
        {
            if (inventory == null || !inventory.Any())
            {
                _logger.LogInformation("No inventory to process from Mahak");
                return;
            }
            
            _logger.LogInformation("Processing {Count} inventory records from Mahak", inventory.Count);
            
            int updated = 0;
            int errors = 0;

            // Group by ProductDetailId and sum quantities
            var inventoryByProduct = inventory
                .Where(i => !i.Deleted)
                .GroupBy(i => i.ProductDetailId)
                .Select(g => new
                {
                    ProductDetailId = g.Key,
                    TotalQuantity = (int)g.Sum(i => i.Count1) // Using Count1 as main quantity
                })
                .ToList();

            foreach (var inv in inventoryByProduct)
            {
                try
                {
                    // Find product by ProductDetailId (which maps to ProductId in Mahak)
                    // Note: This assumes ProductDetailId corresponds to ProductId
                    // You may need to adjust this based on actual Mahak data structure
                    var productMapping = await _mahakMappingRepository.GetByMahakEntityIdAsync(
                        "Product", 
                        inv.ProductDetailId, 
                        cancellationToken);

                    if (productMapping == null)
                    {
                        _logger.LogDebug("Product not found for inventory ProductDetailId: {DetailId}", inv.ProductDetailId);
                        continue;
                    }

                    var product = await _productRepository.GetByIdAsync(productMapping.LocalEntityId, cancellationToken);
                    
                    if (product == null)
                    {
                        _logger.LogWarning("Product entity not found for mapping {MappingId}", productMapping.Id);
                        continue;
                    }

                    product.SetStockQuantity(inv.TotalQuantity);
                    _logger.LogDebug("Updated stock for product {ProductId}: {Quantity}", 
                        inv.ProductDetailId, inv.TotalQuantity);

                    await _productRepository.UpdateAsync(product, cancellationToken);
                    updated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing inventory for ProductDetailId {DetailId}: {Error}", 
                        inv.ProductDetailId, ex.Message);
                    errors++;
                }
            }

            _logger.LogInformation(
                "Inventory sync completed: {Updated} updated, {Errors} errors", 
                updated, errors);
        }
