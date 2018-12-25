jQuery(function(){
    jQuery("#sidekf").hover(function(){
        jQuery("#sidekf .tel").show();
        jQuery(this).animate({ right: "12px" }, 200);
    }, function(){
        jQuery("#sidekf .tel").hide();
        jQuery(this).animate({right: "-110px"}, 200);
    });
});