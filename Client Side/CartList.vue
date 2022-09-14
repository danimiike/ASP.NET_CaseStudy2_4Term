<template>
    <div class="container">
        <img :src="`/store.png`" class="store-image" />
        <div class="heading">Your Orders</div>
        <div class="status">{{ state.status }}</div>
        <div id="cart-list">
            <DataTable
                v-if="state.carts.length > 0"
                :value="state.carts"
                :scrollable="true"
                scrollHeight="60vh"
                dataKey="id"
                class="p-datatable-striped"
                v-model:selection="state.cartSelected"
                selectionMode="single"
                @row-select="onRowSelect"
            >
                <Column header="Order #" field="id" />
                <Column header="Date">
                    <template #body="slotProps">
                        {{ formatDate(slotProps.data.orderDate) }}
                    </template>
                </Column>
            </DataTable>
            <Dialog v-model:visible="state.selectedACart" class="dialog-border">
                <div
                    style="font-weight: bold; font-size: large;text-align: left;"
                    class="cart-head"   
                >
                    ORDER: #{{ state.cartDetails[0].orderId}} &emsp;&emsp; {{ state.cartDetails[0].dateCreated}}
                </div>
                <div class="imgcontainer" align="middle" style="padding:5vw">
                    <img :src="`/store.png`" class="store-image"  />
                </div>
                
                <div class="cartTableHeading" style="font-weight: bold;padding-bottom: inherit;text-align: center;"
                >Quantities</div>
                <DataTable
                    :value="state.cartDetails"
                    :scrollable="true"
                    scrollHeight="38vh"
                    dataKey="id"
                    class="p-datatable-striped"
                    selectionMode="single"
                  
                >
                    <Column header="Name" field="name" style="min-width:25vw; font-size: 3vw"/>
                    <Column id="QtyS" header="S" field="qtyS" style="min-width:0vw; font-size: 3vw;"/>
                    <Column id="QtyO" header="O" field="qtyO" style="min-width:0vw; font-size: 3vw;"/>
                    <Column id="QtyB" header="B" field="qtyB" style="min-width:0vw; font-size: 3vw;"/>
                    <Column id="Extended" header="Extended" field="extended" style="min-width:20vw; font-size: 3vw;"/> 
                </DataTable>
                <table>
				    <tr>
					    <td style="width: 20%; font-weight: bold; font-size: 4.0vw; text-align: right;padding-top: 3vw;padding-bottom: 4vw">Subtotal:</td>
					    <td style="width: 10%; text-align: right; padding-right: 3vw; font-size: 4.0vw;"> {{ formatCurrency(state.subtotal) }}</td>
				    </tr>
				    <tr>
				    	<td style="width: 20%; font-weight: bold; font-size: 4.0vw; text-align: right;padding-bottom: 4vw">Tax:</td>
				    	<td	style="width: 10%; text-align: right; padding-right: 3vw; padding-bottom: 4vw; font-size: 4.0vw;">{{ formatCurrency(state.tax) }}</td>
				    </tr>
				    <tr>
				    	<td style="width: 20%; font-weight: bold; font-size: 4.0vw; text-align: right;padding-bottom: 3vw">Total:</td>
				    	<td style=" width: 10%; text-align: right; padding-right: 3vw; padding-bottom: 3vw; font-size: 4.0vw;">{{ formatCurrency(state.total) }}</td>
				    </tr>
			    </table>
            </Dialog>
        </div>
    </div>
</template>
<script>
import { reactive, onMounted } from "vue";
import { fetcher } from "../util/apiutil";
export default {
    setup() {
        let state = reactive({
            status: "",
            dialogStatus: "",
            carts: [],
            selectedACart: false,
            cartSelected: {},
            cartDetails: [],
            subtotal: 0,
			tax: 0,
			total: 0,
            extended: 0,
        });
        onMounted(() => {
            loadCarts();
           
        });
        const calcExtended = () => {
			state.extended = `${formatCurrency(
				state.cartDetails.extended
		)}`;
		};
        const loadCarts = async () => {
            try {
                let customer = JSON.parse(sessionStorage.getItem("customer"));
                const payload = await fetcher(`order/${customer.email}`);
                if (payload.error === undefined) {
                    state.carts = payload;
                    state.status = `loaded ${state.carts.length} orders`;
                    state.selectedACart = false;
                } else {
                    state.status = payload.error;
                }
            } catch (err) {
                console.log(err);
                state.status = `Error has occured: ${err.message}`;
            }
        };
        const formatDate = (date) => {
            let d;
            // see if date is coming from server
            date === undefined
                ? (d = new Date())
                : (d = new Date(Date.parse(date))); // from server
            let _day = d.getDate();
            let _month = d.getMonth() + 1;
            let _year = d.getFullYear();
            let _hour = d.getHours();
            let _min = d.getMinutes();
            if (_min < 10) {
                _min = "0" + _min;
            }
            return _year + "-" + _month + "-" + _day;
        };
        const onRowSelect = async (event) => {
            state.tax  = 0;
            state.subtotal= 0;
            state.total= 0;
            try {
                
                let customer = JSON.parse(sessionStorage.getItem("customer"));
                state.cartSelected = event.data;
                const payload = await fetcher(
                    `order/${state.cartSelected.id}/${customer.email}`
                );
                state.cartDetails = payload;
                
                state.cartDetails.map((Array) => {
                    state.subtotal += Array.extended;			   
			    });
                state.cartDetails.map((Array) => {
                    Array.extended = formatCurrency(Array.extended);   
			    });
			    state.tax = state.subtotal * 0.13;
			    state.total = state.subtotal + state.tax;

                state.dialogStatus = `details for order ${state.cartSelected.id}`;
                state.selectedACart = true;
            } catch (err) {
                console.log(err);
                state.status = `Error has occured: ${err.message}`;
            }
        };

	    const formatCurrency = (value) => {
			return value.toLocaleString("en-US", {
				style: "currency",
				currency: "USD",
			});
		};
      
        return {
            state,
            calcExtended,
            formatDate,
            onRowSelect,
            formatCurrency,
            
        };
    },
};
</script>