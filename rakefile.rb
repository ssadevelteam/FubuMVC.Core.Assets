begin
  require 'bundler/setup'
  require 'fuburake'
rescue LoadError
  puts 'Bundler and all the gems need to be installed prior to running this rake script. Installing...'
  system("gem install bundler --source http://rubygems.org")
  sh 'bundle install'
  system("bundle exec rake", *ARGV)
  exit 0
end


FubuRake::Solution.new do |sln|
	sln.compile = {
		:solutionfile => 'src/FubuMVC.Core.Assets.sln'
	}
				 
	sln.assembly_info = {
		:product_name => "FubuMVC", # that is correct
		:copyright => 'Copyright 2010-2013 Jeremy D. Miller, Josh Arnold, et al. All rights reserved.'
	}
	
	sln.ripple_enabled = true
	sln.fubudocs_enabled = true
	
	sln.assembly_bottle 'FubuMVC.Core.Assets'
end
