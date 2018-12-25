jQuery(document).ready(function($) {
	$('[name^=progress_bar]').each(function(){
		var percent = $(this).attr('title');
		$(this).animate({width: percent},1000);
	});
	

	$("#btn-wallet").click(function() {
		$('.walletNav').stop().slideToggle();
		return false;
	});
	
	$(window).resize(function() {
		$('.walletNav').removeAttr('style')
	});

});

