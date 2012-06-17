<%@ Page Title="Title" Language="C#" Inherits="System.Web.Mvc.ViewPage<ZpdWebClient.ZPDService.ZpdCurrentPlayerState>" %>
<!DOCTYPE html>
<html class="ui-mobile">
    <head>
        	<meta charset="utf-8"/>
	        <meta name="viewport" content="width=device-width, initial-scale=1"/>
            <title>Welcome to ZPD. We are the next Jukebox</title>

            <link rel="stylesheet" type="text/css" href="<%= Url.Content("~/Content/jquery.mobile-1.1.0.css") %>" />
            <style type="text/css">
                form { font-size: 20px; }
                .evenrow { background-color: #E5E5E5; }
                
                #queue li { background-color: #78a614; }
                #reset { background-color: #cd2c24; }
                
                @media all and (min-width: 750px) {
                    #searchPane { float: left; width: 45%; margin-right: 10%; }
                    #currentStatusPane { float: left; width: 20% }
                }
            </style>

            <script type="text/javascript" src="<%= Url.Content("~/Scripts/jquery-1.7.2.js") %>"></script>
            <script type="text/javascript" src="<%= Url.Content("~/Scripts/jquery.mobile-1.1.0.js") %>"></script>
            <script type="text/javascript">
                var currentTrackId = <%= Model.CurrentTrack.MediaId %>;
                var currentTrackPosition = <%= Model.CurrentTrackPosition %>;
                var currentTrackDuration = <%= Model.CurrentTrack.Duration %>;
                var useMobileVersion = false;

                UpdateCurrentStatus();
                UpdateSongQueue();
                setInterval(UpdateCurrentStatus, 1000);
                setInterval(UpdateSongQueue, 30000);
                function GetFormattedTimespan(seconds) {
                    // create a standard date with no minutes; add seconds, get minutes
                    var d = new Date(2000, 1, 1, 12, 0, seconds, 0);
                    var seconds = d.getSeconds().toString().length < 2 ? "0" + d.getSeconds() : d.getSeconds();
                    return "" + d.getMinutes() + ":" + seconds;
                }
                function UpdateCurrentStatus() {
                    // in case the callback is slow, we assume we're on the same track
                    if (currentTrackPosition < currentTrackDuration) {
                        currentTrackPosition++;
                        UpdateSongPositionDisplay();
                    }
                    $.get('<%= Url.Action("GetCurrentPlayerState") %>', null, function (data) {
                        $('#currentTrackName').html(data.CurrentTrack.Name);
                        $('#currentTrackArtist').html(data.CurrentTrack.Artist);

                        currentTrackPosition = data.CurrentTrackPosition;
                        currentTrackDuration = data.CurrentTrack.Duration;

                        UpdateSongPositionDisplay();

                        if (currentTrackId != data.CurrentTrack.MediaId) {
                            currentTrackId = data.CurrentTrack.MediaId;
                            UpdateSongQueue();
                        }
                    });
                }

                function UpdateSongPositionDisplay() {
                    $('#currentTrackPosition').html(GetFormattedTimespan(currentTrackPosition));
                    $('#currentTrackDuration').html(GetFormattedTimespan(currentTrackDuration));
                }

                function UpdateSongQueue() {
                    $.get('<%= Url.Action("GetCurrentQueue") %>', null, function (data) {
                        var queue = $('#queue');
                        queue.html('');
                        for (var i = 0; i < data.length; i++) {
                            queue.append('<li data-theme="c" data-corners="false" data-shadow="false" data-wrapperEls="div" data-iconpos="right"><h3><strong>' + data[i].Name + '</strong></h3><p><strong>' + data[i].Artist + '</strong></p></li>');
                        }
                        queue.listview('refresh');
                    });
                }

                function PerformSearch() {
                    var query = $("#searchInput").val();
                    $.get('<%= Url.Action("Search") %>?query=' + query, null, function (data) {
                        var results = $("#searchResults");
                        results.html('');
                        for (var i = 0; i < data.length; i++) {
                            results.append('<li id="result' + data[i].MediaId + '" data-theme="c" data-corners="false" data-shadow="false" data-iconshadow="true" data-wrapperEls="div" data-icon="plus" data-iconpos="right"><a class="ui-link-inherit" href="javascript:Enqueue(' + data[i].MediaId + ',' + data[i].MediaTypeId + ')"><h3><strong>' + data[i].Name + '</strong></h3><p><strong>' + data[i].Artist + '</strong></p><p>' + data[i].Album + '</p></a></li>');
                        }
                        results.listview('refresh');
                    });
                }
                function Enqueue(mediaId, mediaTypeId) {
                    $.post('<%= Url.Action("QueueTrack") %>', { MediaId: mediaId, MediaTypeId: mediaTypeId }, function () {
                        $('#result'+mediaId).fadeOut('slow');
                        UpdateSongQueue();
                    });
                }
                function searchKeyPress() {
                    if (window.event && 13 == window.event.keyCode) {
                        // enter pressed
                        PerformSearch();
                    }
                }
                        
    </script>
    </head>
    <body class="ui-mobile-viewport">
    
        <div>
            <div data-role="header">ZPD</div>
	        <div data-role="content">
                <div id="searchPane">
                    <h3>Search</h3>
                        <input type="text" id="searchInput" name="searchInput" size="200" onkeypress="searchKeyPress();"/>
                        <button type="button" onclick="PerformSearch();">Search</button>
                    <ul id="searchResults" class="ui-corner-all ui-shadow" data-role="listview" data-inset="true">
                    </ul>
                    <button id="reset" type="button" onclick="document.getElementById('searchResults').innerHTML=''">Clear</button>
                </div>
                <div id="currentStatusPane">
                    <div>
						<h2>Currently Playing</h2>
						<p><span id="currentTrackName"><%= Model.CurrentTrack.Name %></span> - <span id="currentTrackArtist"><%= Model.CurrentTrack.Artist %></span> (<span id="currentTrackPosition">0:00</span>/<span id="currentTrackDuration">0:00</span>)</p>
					</div>
					<div>
						<h2>Current Queue</h2>
						<ul id="queue" class="ui-corner-all ui-shadow" data-role="listview" data-inset="true">
						</ul>
					</div>
                </div>
            </div>
        </div>
    </body>
</html>