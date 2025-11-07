<?php
$test = false;

if (isset($_GET["test"])) {
	$test = $_GET["test"] == '1';
}
?>

<!DOCTYPE html>
<html>
<head>
<title>Yoba Löncher</title>
<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />
<meta http-equiv="X-UA-Compatible" content="IE=11" />
<link type="text/css" rel="stylesheet" href="main.css" />
<script language='JavaScript' src='assets/jq' type='text/javascript'></script>
<script language='JavaScript' src='assets/ts' type='text/javascript'></script>
<script language='JavaScript' src='assets/bb' type='text/javascript'></script>
<script language='JavaScript' src='main.js' type='text/javascript'></script>

<?php
	echo $test ? "<script language='JavaScript' src='assets/yobalib_dummy.js' type='text/javascript'></script>" : "<script>;[[[YOBALIB]]];</script>";

	$donate = file_get_contents('donate.html');

	$links = file_get_contents('links.html');
	$headerContent = "";
	if ($links == false) {
		$links = "Ссылки отсутствуют.";
	} else {
		$pageSplit = explode("<!---/HEAD---->", $links);
		if (count($pageSplit) > 1) {
			$headerContent = $pageSplit[0];
			$links = $pageSplit[1];
		}
	}

	if ($donate == false) {
		$donate = "Сегодня донаты не принимаем.";
	} else {
		$pageSplit = explode("<!---/HEAD---->", $donate);
		if (count($pageSplit) > 1) {
			$headerContent .= $pageSplit[0];
			$donate = $pageSplit[1];
		}
	}

	echo $headerContent;
?>

<script>

<?php
	echo "var TESTMODE = ".($test ? "true" : "false").";";
	$changelog = file_get_contents('changelog.html');
	if ($changelog != false) {
		$changelog = str_replace('\\', '\\\\', $changelog);
		$changelog = str_replace("\n", "\\\n", $changelog);
		$changelog = str_replace("\r", "", $changelog);
		$changelog = str_replace('\'', '\\\'', $changelog);
		echo "var changelograw = '".$changelog."';";
	}
	else {
		echo "var changelograw = 'Ошибка\\\n=====Content=====\\\n<h1>Ошибка</h1><br><h2>Не удалось прочитать файл ченджлога</h2>';";
	}

	$faq = file_get_contents('faq.html');
	if ($faq != false) {
		$faq = str_replace('\\', '\\\\', $faq);
		$faq = str_replace("\n", "\\\n", $faq);
		$faq = str_replace("\r", "", $faq);
		$faq = str_replace('\'', '\\\'', $faq);
		echo "var faqraw = '".$faq."';";
	}
	else {
		echo "var faqraw = '<h1>Ошибка</h1><br><h2>Не удалось прочитать файл FAQ</h2>';";
	}
?>

YLExtInit()

</script>

</head>
<body>

<div class='progress-status'><div class='caption'></div></div>

<div class='progress'>
	<div class="border"><div class="border-mid"></div></div>
	<div class="bar">
		<div class="start"></div>
		<div class="mid"></div>
		<div class="end"></div>
	</div>
</div>

<div id='LaunchBtn' class='app-btn' tabindex='1'><div class='caption'></div></div>

<div class='app-buttons'>
</div>

<div class='app-content-pages'>
	<div class='page' id='ChangelogView'>

		<div class='kb-side-menu'></div>

		<div class="articleContentWrapper">
			<div class='article-content'></div>
		</div>

	</div>

	<div class='page' id='ModsView'>
		<div class="articleContentWrapper">
			<div class='article-content'></div>
		</div>
	</div>

	<div class='page' id='FAQView'>
		<div class="articleContentWrapper">
			<div class='article-content'></div>
		</div>
	</div>

	<div class='page' id='DonateView'>
		<div class="articleContentWrapper">
			<div class='article-content'><?php echo $donate; ?></div>
		</div>
	</div>

	<div class='page' id='LinksView'>
		<div class="articleContentWrapper">
			<div class='article-content'><?php echo $links; ?></div>
		</div>
	</div>

	<div class='page' id='StatusView'>
		<div class="articleContentWrapper">
			<div class='article-content'><h2>Подготовка...</h2></div>
		</div>
	</div>

	<div class='page' id='SettingsView'>
		<div class='articleContentWrapper'>
			<div class='article-content'></div>
		</div>
	</div>
</div>

<div class='preloader'></div>
<div class='app-controls'>
	<div class='app-control-btn close' onclick="YL.AppClose()" tabindex='212'></div>
	<div class='app-control-btn minimize' onclick="YL.AppMinimize()" tabindex='211'></div>
	<div class='app-control-btn help' onclick="YL.AppHelp()" tabindex='210'></div>
</div>

</body>
</html>