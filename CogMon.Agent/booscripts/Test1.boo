import System

sql_select("atmodb", "select id, name from activityactiontype", {at|
    #print("${at.id} => ${at.name}");
})

add_data_record((1.0, 2.3, 34.3))