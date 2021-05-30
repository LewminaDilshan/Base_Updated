# Readline Lanka ERP Application

* Please create a new branch from **develop branch** before doing any changes to the sourse code.

* **Please create different branches for your tasks.**

## Naming Conventions for Branches
Example: (author)_(branch-type)_(branch-name/module_name)

	Branch Type :
	feat: (new feature for the user, not a new feature for build script)
	fix: (bug fix for the user, not a fix to a build script)
	docs: (changes to the documentation)
	style: (formatting, missing semi colons, etc; no production code change)
	refactor: (refactoring production code, eg. renaming a variable)
	test: (adding missing tests, refactoring tests; no production code change)
	chore: (updating grunt tasks etc; no production code change)

* After your implementation please commit the changes with a proper commit massage.

* **Before pushing your branch please pull from remote develop branch for getting incomming changes.**

* After commiting your changes push the branch to the remote directory.

* Do not push the build generated files such as **bin, obj, .pub and log files.** Before pushing please stash them.

* Specialy don't push your **Web.config** file to the remote branch.

* **Do not push to the master or develop branch directly.**

* After pushing your branch please create a **pull request** to the **develop** branch.
