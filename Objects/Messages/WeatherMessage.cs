using ForecasterText.Objects.Enums;
using StardewModdingAPI;
using StardewValley;

namespace ForecasterText.Objects.Messages {
    public abstract class WeatherMessage : ISourceMessage {
        protected abstract string T9N { get; }
        
        protected WeatherMessage() {}
        
        protected abstract int GetWeather();
        protected abstract WeatherDisplay GetDisplay(ForecasterConfig config);
        protected virtual bool ShouldDisplayFor(Farmer farmer) => true;
        
        /// <inheritdoc />
        public string Write(Farmer farmer, ITranslationHelper t9N, ForecasterConfig config) {
            if (!this.ShouldDisplayFor(farmer))
                return null;
            
            WeatherDisplay display = this.GetDisplay(config);
            int weather = this.GetWeather();
            
            if (!this.ShowWeather(display, weather))
                return null;
            
            WeatherIcons[] emojis = weather switch {
                Game1.weather_sunny
                    => new[] { WeatherIcons.SUN },
                Game1.weather_festival
                    => new[] { WeatherIcons.SUN, WeatherIcons.FESTIVAL },
                Game1.weather_wedding
                    => new[] { WeatherIcons.SUN, WeatherIcons.WEDDING },
                Game1.weather_rain
                    => new[] { WeatherIcons.RAIN },
                Game1.weather_debris when Game1.currentSeason.Equals("winter")
                    => new[] { WeatherIcons.SNOW },
                Game1.weather_debris
                    => new[] { WeatherIcons.SUN },
                Game1.weather_lightning
                    => new[] { WeatherIcons.LIGHTNING, WeatherIcons.RAIN },
                Game1.weather_snow
                    => new[] { WeatherIcons.SNOW },
                _ => null
            };
            
            // If no icons, or no results
            if (emojis is not {Length: >0})
                return null;
            
            MessageBuilder builder = new(this.T9N);
            foreach (WeatherIcons emoji in emojis)
                builder.AddEmoji("...", emoji);
            
            return MessageSource.TV(builder)
                .Write(farmer, t9N, config);
        }
        
        protected virtual bool ShowWeather(WeatherDisplay display, int weather) {
            return display switch {
                WeatherDisplay.NEVER => false,
                WeatherDisplay.ALWAYS => true,
                _ => weather switch {
                    Game1.weather_sunny => display is WeatherDisplay.NOT_RAINING,
                    Game1.weather_festival => display is WeatherDisplay.NOT_RAINING,
                    Game1.weather_wedding => display is WeatherDisplay.NOT_RAINING,
                    Game1.weather_rain => display is WeatherDisplay.RAINING,
                    Game1.weather_debris => display is WeatherDisplay.NOT_RAINING,
                    Game1.weather_lightning => display is WeatherDisplay.RAINING,
                    Game1.weather_snow => display is WeatherDisplay.NOT_RAINING,
                    _ => false
                }
            };
        }
        
        public class TestDisplay : PelicanTown {
            private readonly WeatherIcons Weather;
            
            public TestDisplay(WeatherIcons weather): base() {
                this.Weather = weather;
            }
            
            /// <inheritdoc />
            protected override bool ShouldDisplayFor(Farmer farmer)
                => true;
            
            /// <inheritdoc />
            protected override int GetWeather()
                => (int)this.Weather;
            
            /// <inheritdoc />
            protected override bool ShowWeather(WeatherDisplay display, int weather)
                => true;
        }
        public class PelicanTown : WeatherMessage {
            protected override string T9N => "tv.weather.pelican_town";
            
            public PelicanTown(): base() {}
            
            /// <inheritdoc />
            protected override int GetWeather() {
                WorldDate date = new(Game1.Date); // Copy the date so we don't modify the origin
                ++date.TotalDays; // Increase by one to get the next days weather
                return !Game1.IsMasterGame ? Game1.getWeatherModificationsForDate(date, Game1.netWorldState.Value.WeatherForTomorrow) : Game1.getWeatherModificationsForDate(date, Game1.weatherForTomorrow);
            }
            
            /// <inheritdoc />
            protected override WeatherDisplay GetDisplay(ForecasterConfig config)
                => config.StardewValleyWeather;
        }
        public class GingerIsland : WeatherMessage {
            protected override string T9N => "tv.weather.ginger_island";
            
            public GingerIsland(): base() {}
            
            /// <inheritdoc />
            protected override int GetWeather() {
                return Game1.netWorldState.Value.GetWeatherForLocation(
                    Game1.getLocationFromName("IslandSouth").GetLocationContext()
                ).weatherForTomorrow.Value;
            }
            
            /// <inheritdoc />
            protected override WeatherDisplay GetDisplay(ForecasterConfig config)
                => config.GingerIslandWeather;
            
            /// <inheritdoc />
            protected override bool ShouldDisplayFor(Farmer farmer)
                => farmer.hasOrWillReceiveMail("Visited_Island");
        }
    }
}
