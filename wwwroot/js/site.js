// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $('.btn-product-import').click(function () {
        var count = $('#txt-product-count').val();
        if (!count) {
            $('.txt-product-count-error').show();
        }
        else {
            $('.txt-product-count-error').hide();
            $.get('/api/products/fakeimport/' + count, function () {
                location.reload();
            });
        }  
    });

    $('.btn-product-reindex').click(function () {
        $.get('/api/search/reindex', function () {
            location.reload();
        });
    });

    $('.btn-product-delete').click(function () {
        var that = this;
        $.ajax({
            url: '/api/products/' + $(that).data('id'),
            type: 'DELETE',
            success: function (result) {
                $(that).closest('tr').remove();
            }
        });
    });

    function createProductGroupItem(element) {
        var itemTpl = $($('#productgroupitem').html());
        itemTpl.find('.group-name').html(element.key);
        itemTpl.find('.group-count').html(element.value);
        return itemTpl;
    }

    $('#txt-product-search').keyup(function () {
        if ($(this).val().length >= 2) {
            $.get('/api/search/find?query=' + $(this).val(), function (data) {
                $('.search-result').html('');
                $(data).each(function (index, element) {
                    var itemTpl = $($('#searchitem').html());
                    itemTpl.find('.product-image > img').attr('src', element.image);
                    itemTpl.find('.product-name').html(element.name);
                    itemTpl.find('.product-description').html(element.description);
                    itemTpl.find('.product-category').html(element.category.name);
                    itemTpl.find('.product-brand').html(element.brand.name);
                    $('.search-result').append(itemTpl);
                })
                $('.search-result').show();
            });

            $.get('/api/search/aggregations?query=' + $(this).val(), function (data) {
                $('.min-price-txt').html(data.min_price);
                $('.avg-price-txt').html(data.average_price);
                $('.max-price-txt').html(data.max_price);
                $('.categories-products-list').html('');
                $('.brands-products-list').html('');
                $('.stores-products-list').html('');
                $(data.products_for_category).each(function (index, element) {
                    $('.categories-products-list').append(createProductGroupItem(element));
                });
                $(data.products_for_brand).each(function (index, element) {
                    $('.brands-products-list').append(createProductGroupItem(element));
                });
                $(data.products_for_store).each(function (index, element) {
                    $('.stores-products-list').append(createProductGroupItem(element));
                });
            });
        }
        else {
            $('.search-result').hide();
        }
    });
    $('#txt-product-search').click(function () {
        $('.search-result').toggle();
    })

})
