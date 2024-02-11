all: modules

modules:
	# NPM modules from the package-lock.json file
	npm install

deploy: modules
	git push --tags	# also push to github repository, don't deploy only to the prod server
	git push production
