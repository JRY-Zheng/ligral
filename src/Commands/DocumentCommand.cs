/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

namespace Ligral.Commands
{
    class DocumentCommand : Command
    {
        public string ModelName;
        public bool? ToJson;
        public string OutputFolder;
        public override string HelpInfo {get => @"Command: doc & document
    Position parameter:
        ModelName           optional string
            if exist        return the document of this specific model.
            else            return documents of all models.
    Named parameters:
        --json & -j         boolean
            if true         output the definition(s) in JSON format.
            else            print document(s) on the screen.
        --output & -o       string
            if given        output JSON file in the given folder.
            else            output JSON file in the startup folder.
    Examples:
        ligral doc          print all documents on the screen
        ligral doc --json -o def
                            output all JSON definitions to the `def` folder.
        ligral doc Sin      print the document of the model `Sin`.
";}
    }
}