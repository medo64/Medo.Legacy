-- Copyright (c) 2010 Josip Medved <jmedved@jmedved.com>



do
    local tinymessage = Proto("tinymessage", "TinyMessage Protocol")


    local f = tinymessage.fields
    f.version = ProtoField.string("tinymessage.version", "Version")
    f.product = ProtoField.string("tinymessage.product", "Product")
    f.operation = ProtoField.string("tinymessage.operation", "Operation")
    f.data = ProtoField.string("tinymessage.data", "Data")


    function tinymessage.init()
    end


    function tinymessage.dissector(buffer, pinfo, tree)
        local subtree = tree:add(tinymessage, buffer())
        
        local count = buffer:len()
        local start = 0
        local index = 0

        while (index < count) do
            if (buffer(index,1):uint()==0x20) then break end
            index = (index+1)
        end
        if (index > count) then
            subtree:add(f.id, "")
            subtree:add(f.product, "")
            subtree:add(f.operation, "")
            subtree:add(f.data, "")
            return
        end

        local valueId = buffer(start, index - start):string()
        if (valueId == "Tiny") then
            subtree:add(f.version, buffer(start, index - start), "1")
        else
            subtree:add(f.version, buffer(start, index - start), "Unknown version (" .. valueId .. ")")
        end


        index = (index + 1)
        start = index;
        while (index < count) do
            if (buffer(index,1):uint()==0x20) then break end
            index = (index+1)
        end
        if (index > count) then
            subtree:add(f.product, "")
            subtree:add(f.operation, "")
            subtree:add(f.data, "")
            return
        end

        local valueProduct = buffer(start, index - start):string()
        subtree:add(f.product, buffer(start, index - start), valueProduct)


        index = (index + 1)
        start = index;
        while (index < count) do
            if (buffer(index,1):uint()==0x20) then break end
            index = (index+1)
        end
        if (index > count) then
            subtree:add(f.operation, "")
            subtree:add(f.data, "")
            return
        end

        local valueOperation = buffer(start, index - start):string()
        subtree:add(f.operation, buffer(start, index - start), valueOperation)


        start = (index + 1);
        index = count;
        if (index > count) then
            subtree:add(f.data, "")
            return
        end

        subtree:add(f.data, buffer(start, index - start))


        subtree:append_text (", " .. valueProduct .. " " .. valueOperation)
        
        pinfo.cols.protocol = "TINYMESSAGE"
        pinfo.cols.info = valueProduct .. " " .. valueOperation
    end


    local udp_table = DissectorTable.get("udp.port")
    udp_table:add(5104, tinymessage)
end